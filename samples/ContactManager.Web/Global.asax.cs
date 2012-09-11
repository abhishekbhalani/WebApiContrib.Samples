﻿using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using ContactManager.Models;
using ContactManager.Web.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Ninject;
using WebApiContrib.Filters;
using WebApiContrib.Formatting;
using WebApiContrib.IoC.Ninject;
using WebApiContrib.MessageHandlers;
using WebApiContrib.Selectors;

namespace ContactManager.Web
{
    public class MvcApplication : HttpApplication
    {
        public static void RegisterApis(HttpConfiguration config)
        {
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Converters.Add(new IsoDateTimeConverter());

            config.Filters.Add(new ExceptionHandlingAttribute());
            config.Filters.Add(new ValidationAttribute());

            config.Formatters.Add(new ProtoBufFormatter()); 
            config.Formatters.Add(new ContactPngFormatter());
            config.Formatters.Add(new VCardFormatter());
            config.Formatters.Add(new ContactCalendarFormatter());
            
            config.MessageHandlers.Add(new UriFormatExtensionHandler(new UriExtensionMappings()));

            //var loggingRepo = (ILoggingRepository) config.DependencyResolver.GetService(typeof(ILoggingRepository));
            //config.MessageHandlers.Add(new LoggingHandler(loggingRepo));

            config.MessageHandlers.Add(new NotAcceptableMessageHandler(config));

            ConfigureResolver(config);

            config.Routes.MapHttpRoute(
                "Default",
                "{controller}/{id}/{ext}",
                new { id = RouteParameter.Optional, ext = RouteParameter.Optional });
        }

        private static void ConfigureResolver(HttpConfiguration config)
        {
            var kernel = new StandardKernel();
            kernel.Bind<IContactRepository>().ToConstant(new InMemoryContactRepository());
            kernel.Bind<IHttpActionSelector>().ToConstant(new CorsActionSelector());

        	config.DependencyResolver = new NinjectResolver(kernel);
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        protected void Application_Start()
        {
            RegisterApis(GlobalConfiguration.Configuration);
            RegisterGlobalFilters(GlobalFilters.Filters);
        }
    }
}
