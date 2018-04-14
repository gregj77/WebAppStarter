(function () {
    var rootUrl = window.location.href;
    var index = rootUrl.toLowerCase().indexOf('/swagger/ui');
    var authUrl = rootUrl.substring(0, index) + '/api/token';
    var LoginData = Backbone.View.extend({
        render: function () {

            var target = this.$el;
            var userName = $('<input id="userName" type="text" name="userName" placeholder="username">');
            var password = $('<input id="password" type="password" name="password" placeholder="password">');
            var login = $('<button type="button">Login</button>');
            var logout = $('<button type="button">Logout</button>');
            var result = $('<p></p>');
            userName
                .css('margin', '0')
                .css('border', '1px solid gray')
                .css('padding', '3px')
                .css('width', '100px')
                .css('font-size', '0.9em');

            password
                .css('margin', '0')
                .css('border', '1px solid gray')
                .css('padding', '3px')
                .css('width', '100px')
                .css('font-size', '0.9em');

            login
                .css('margin', '0')
                .css('border', '1px solid gray')
                .css('padding', '3px')
                .css('width', '75px')
                .css('font-size', '0.9em');

            logout
                .css('margin', '0')
                .css('border', '1px solid gray')
                .css('padding', '3px')
                .css('width', '75px')
                .css('font-size', '0.9em');

            logout.hide();

            result
                .css('color', 'red')
                .css('font-size', '0.9em');

            login.click(function() {
                var name = userName.val();
                var pass = password.val();

                $.ajax({
                        type: 'POST',
                        url: authUrl,
                        data: {
                            grantType: 'credentials',
                            username: name,
                            password: pass
                        },
                        success: function (data) {
                            var key = 'Bearer ' + data.access_token;
                            swaggerUi.api.clientAuthorizations.add("_authKey", new SwaggerClient.ApiKeyAuthorization("Authorization", key, "header"));
                            logout.show();
                            login.hide();
                            password.val('');
                            userName.hide();
                            password.hide();
                            result.text('');
                            result.hide();
                        }
                    })
                    .fail(function () {
                        result.text('Failed to authorize user!');
                    });
            });

            logout.click(function() {
                logout.hide();
                login.show();
                userName.show();
                password.show();
                result.show();
                swaggerUi.api.clientAuthorizations.remove("_authKey");
            });
                

            target.empty();
            target.append(userName);
            target.append(password);
            target.append(login);
            target.append(logout);
            target.append(result);
            return this;
        }
    });

    new LoginData({ el: $('#api_selector div:nth-child(2)') }).render();
})();