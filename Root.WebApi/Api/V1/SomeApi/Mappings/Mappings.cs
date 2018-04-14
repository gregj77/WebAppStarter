using System;
using System.ComponentModel;
using System.Globalization;
using Api.Setup.Modules;
using Autofac;
using Feature01.Models;
using Nelibur.ObjectMapper;
using SomeApi.Models;

namespace SomeApi.Mappings
{
    internal class Mappings : TinyMapperConfigurationBaseModule
    {
        protected override void RegisterMappings(ContainerBuilder builder)
        {
            TinyMapper.Bind<Device, DeviceModel>(cfg =>
            {
                cfg.Bind(souruce => souruce.Id, target => target.DeviceId);
                cfg.Bind(source => source.DeviceName, target => target.Name);
                cfg.Bind(source => source.Description, target => target.Description);
            });
            TinyMapper.Bind<DeviceModel, Device>(cfg =>
            {
                cfg.Bind(souruce => souruce.DeviceId, target => target.Id);
                cfg.Bind(source => source.Name, target => target.DeviceName);
                cfg.Bind(source => source.Description, target => target.Description);
            });
            //TypeDescriptor.AddAttributes(typeof(SomeDummyType), new TypeConverterAttribute(typeof(SomeDummyTypeConverter)));
        }

        #region Type conversion

        private class SomeDummyTypeConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return false;
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                return value;
            }
        }


        #endregion
    }
}