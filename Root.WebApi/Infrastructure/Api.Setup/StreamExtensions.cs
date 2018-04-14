using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using FluentValidation;
using Nelibur.ObjectMapper;
using Utils;

namespace Api.Setup
{
    public static class ObservableWebExtensions
    {
        public static Task<IHttpActionResult> ToCreatedResult<TResult>(this IObservable<TResult> stream, ApiController ctrl, string routeName, Func<TResult, object> routeArgsCreator)
        {
            Func<TResult, ApiController, IHttpActionResult> createResult = (result, c) =>
            {
                var urlHelper = new UrlHelper(ctrl.Request);
                var routeArgs = routeArgsCreator(result);
                var link = new Uri(urlHelper.Link(routeName, routeArgs));

                return new CreatedNegotiatedContentResult<TResult>(link, result, ctrl);
            };
            return stream.ToResultInternal(ctrl, createResult);
        }

        public static Task<IHttpActionResult> ToContentResult<TResult>(this IObservable<TResult> stream, ApiController ctrl)
        {
            return stream.ToResultInternal(ctrl, CreateContentResult);
        }

        public static IObservable<TOutput> Remap<TInput, TOutput>(this IObservable<TInput> stream)
        {
            return stream.Select(obj => TinyMapper.Map<TInput, TOutput>(obj));
        }

        public static IObservable<IEnumerable<TOutput>> RemapColletion<TInput, TOutput>(this IObservable<IEnumerable<TInput>> stream)
        {
            return stream
                .Select(inputCollection => inputCollection
                    .Select(input => TinyMapper.Map<TInput, TOutput>(input))
                    .ToArray());
        }

        private static Task<IHttpActionResult> ToResultInternal<TResult>(
            this IObservable<TResult> stream,
            ApiController ctrl,
            Func<TResult, ApiController, IHttpActionResult> createDelegate)
        {
            var tcs = new TaskCompletionSource<IHttpActionResult>();

            stream
                .Where(t => !Unit.Default.Equals(t))
                .TakeLast(1)
                .Subscribe(
                    result =>
                    {
                        var retValue = createDelegate(result, ctrl);
                        tcs.SetResult(retValue);
                    },
                    error =>
                    {
                        if (HandleArgumentException(ctrl, error, tcs)) return;

                        if (HandleBusinessException(ctrl, error, tcs)) return;

                        if (HandleOperationException(ctrl, error, tcs)) return;

                        if (HandleDataConstraintException(ctrl, error, tcs)) return;

                        if (HandleDataNotFoundException(error, tcs)) return;

                        if (HandleTooMuchDataException(error, tcs)) return;

                        if (HandleValidationException(ctrl, error, tcs)) return;

                        if (HandleUnauthorizedException(ctrl, error, tcs)) return;

                        tcs.SetException(error);
                    },
                    () =>
                    {
                        tcs.TrySetResult(new StatusCodeResult(HttpStatusCode.OK, ctrl));
                    });

            return tcs.Task;
        }

        private static bool HandleValidationException(ApiController ctrl, Exception error, TaskCompletionSource<IHttpActionResult> tcs)
        {
            var validationError = error as ValidationException;
            if (null != validationError)
            {
                var response = new ValidationErrorResponse
                {
                    Message = error.Message,
                    Details = validationError.Errors.Select(p => p.ErrorMessage).ToArray()
                };
                tcs.TrySetResult(new NegotiatedContentResult<ValidationErrorResponse>((HttpStatusCode)422, response, ctrl));
                return true;
            }
            return false;
        }

        private static bool HandleTooMuchDataException(Exception error, TaskCompletionSource<IHttpActionResult> tcs)
        {
            var tooMuchData = error as TooMuchDataFoundException;
            if (null != tooMuchData)
            {
                tcs.TrySetResult(
                    new ResponseMessageResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        ReasonPhrase = "Not unique data found",
                        Content = new StringContent(tooMuchData.Message)
                    }));
                return true;
            }
            return false;
        }

        private static bool HandleDataNotFoundException(Exception error, TaskCompletionSource<IHttpActionResult> tcs)
        {
            var notFoundEx = error as DataNotFoundException;
            if (null != notFoundEx)
            {
                tcs.TrySetResult(new ResponseMessageResult(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Data not found",
                    Content = new StringContent(notFoundEx.Message)
                }));
                return true;
            }
            return false;
        }

        private static bool HandleDataConstraintException(ApiController ctrl, Exception error, TaskCompletionSource<IHttpActionResult> tcs)
        {
            var sqlViolationError = error as UniqueKeyViolationException;
            if (null != sqlViolationError)
            {
                var response = new ValidationErrorResponse
                {
                    Message = "Unique constraint violation",
                    Details = new[] { sqlViolationError.Message }
                };
                tcs.TrySetResult(new NegotiatedContentResult<ValidationErrorResponse>((HttpStatusCode)422, response, ctrl));
                return true;
            }
            return false;
        }

        private static bool HandleBusinessException(ApiController ctrl, Exception error, TaskCompletionSource<IHttpActionResult> tcs)
        {
            var businessError = error as BusinessException;
            if (null != businessError)
            {
                tcs.TrySetResult(new ResponseMessageResult(new HttpResponseMessage((HttpStatusCode)422)
                {
                    ReasonPhrase = businessError.Message,
                    Content =
                        new ObjectContent(businessError.Content.GetType(), businessError.Content,
                            ctrl.Configuration.Formatters.JsonFormatter)
                }));
                return true;
            }
            return false;
        }

        private static bool HandleOperationException(ApiController ctrl, Exception error, TaskCompletionSource<IHttpActionResult> tcs)
        {
            var operationException = error as OperationException;
            if (null != operationException)
            {
                tcs.TrySetResult(new ResponseMessageResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = operationException.Message,
                    Content = new ObjectContent(operationException.Content.GetType(), operationException.Content, ctrl.Configuration.Formatters.JsonFormatter)
                }));
                return true;
            }
            return false;
        }

        private static bool HandleArgumentException(ApiController ctrl, Exception error, TaskCompletionSource<IHttpActionResult> tcs)
        {
            var argNull = error as ArgumentException;
            if (null != argNull)
            {
                tcs.TrySetResult(new BadRequestResult(ctrl));
                return true;
            }
            return false;
        }

        private static bool HandleUnauthorizedException(ApiController ctrl, Exception error, TaskCompletionSource<IHttpActionResult> tcs)
        {
            var accessError = error as UnauthorizedAccessException;
            if (null != accessError)
            {
                tcs.TrySetResult(new ResponseMessageResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    ReasonPhrase = accessError.Message,
                    Content = new ObjectContent(accessError.GetType(), accessError, ctrl.Configuration.Formatters.JsonFormatter)
                }));
                return true;
            }
            return false;
        }

        private static IHttpActionResult CreateContentResult<TResult>(TResult result, ApiController ctrl)
        {
            return new NegotiatedContentResult<TResult>(HttpStatusCode.OK, result, ctrl);
        }

        public class ValidationErrorResponse
        {
            public string Message { get; set; }
            public string[] Details { get; set; }
        }
    }
}