using System;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Osaka.Api.Infrastructure.Logging
{
    public static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            ElasticCountryAdded = LoggerMessage.Define<int>(LogLevel.Information,
                new EventId(2010, "ElasticCountryAdded"),
                "'{numberOfCountries}' countries have been added");
            
            ElasticLocalityAdded = LoggerMessage.Define<int>(LogLevel.Information,
                new EventId(2011, "ElasticLocalityAdded"),
                "'{numberOfLocalities}' localities have been added");
            
            ElasticAccommodationAdded = LoggerMessage.Define<int>(LogLevel.Information,
                new EventId(2012, "ElasticAccommodationAdded"),
                "'{numberOfAccommodations}' accommodations have been added");
            
            ElasticCountryUpdated = LoggerMessage.Define<int>(LogLevel.Information,
                new EventId(2013, "ElasticCountryUpdated"),
                "'{numberOfCountries}' countries have been added");
            
            ElasticLocalityUpdated = LoggerMessage.Define<int>(LogLevel.Information,
                new EventId(2014, "ElasticLocalityUpdated"),
                "'{numberOfLocalities}' localities have been added");
            
            ElasticAccommodationUpdated = LoggerMessage.Define<int>(LogLevel.Information,
                new EventId(2015, "ElasticAccommodationUpdated"),
                "'{numberOfAccommodations}' accommodations have been added");
            
            ElasticCountryDeleted = LoggerMessage.Define<int>(LogLevel.Information,
                new EventId(2016, "ElasticCountryDeleted"),
                "'{numberOfCountries}' countries have been added");
            
            ElasticLocalityDeleted = LoggerMessage.Define<int>(LogLevel.Information,
                new EventId(2017, "ElasticLocalityDeleted"),
                "'{numberOfLocalities}' localities have been added");
            
            ElasticAccommodationDeleted = LoggerMessage.Define<int>(LogLevel.Information,
                new EventId(2018, "ElasticAccommodationDeleted"),
                "'{numberOfAccommodations}' accommodations have been added");
            
            ElasticCountryErrors = LoggerMessage.Define<string>(LogLevel.Critical,
                new EventId(2019, "ElasticCountryErrors"),
                "{errors}");
            
            ElasticLocalityErrors = LoggerMessage.Define<string>(LogLevel.Critical,
                new EventId(2020, "ElasticLocalityErrors"),
                "{errors}");
            
            ElasticAccommodationErrors = LoggerMessage.Define<string>(LogLevel.Critical,
                new EventId(2021, "ElasticAccommodationErrors"),
                "{errors}");
            
        }
    
                
         public static void LogElasticCountryAdded(this ILogger logger, int numberOfCountries, Exception exception = null)
            => ElasticCountryAdded(logger, numberOfCountries, exception);
                
         public static void LogElasticLocalityAdded(this ILogger logger, int numberOfLocalities, Exception exception = null)
            => ElasticLocalityAdded(logger, numberOfLocalities, exception);
                
         public static void LogElasticAccommodationAdded(this ILogger logger, int numberOfAccommodations, Exception exception = null)
            => ElasticAccommodationAdded(logger, numberOfAccommodations, exception);
                
         public static void LogElasticCountryUpdated(this ILogger logger, int numberOfCountries, Exception exception = null)
            => ElasticCountryUpdated(logger, numberOfCountries, exception);
                
         public static void LogElasticLocalityUpdated(this ILogger logger, int numberOfLocalities, Exception exception = null)
            => ElasticLocalityUpdated(logger, numberOfLocalities, exception);
                
         public static void LogElasticAccommodationUpdated(this ILogger logger, int numberOfAccommodations, Exception exception = null)
            => ElasticAccommodationUpdated(logger, numberOfAccommodations, exception);
                
         public static void LogElasticCountryDeleted(this ILogger logger, int numberOfCountries, Exception exception = null)
            => ElasticCountryDeleted(logger, numberOfCountries, exception);
                
         public static void LogElasticLocalityDeleted(this ILogger logger, int numberOfLocalities, Exception exception = null)
            => ElasticLocalityDeleted(logger, numberOfLocalities, exception);
                
         public static void LogElasticAccommodationDeleted(this ILogger logger, int numberOfAccommodations, Exception exception = null)
            => ElasticAccommodationDeleted(logger, numberOfAccommodations, exception);
                
         public static void LogElasticCountryErrors(this ILogger logger, string errors, Exception exception = null)
            => ElasticCountryErrors(logger, errors, exception);
                
         public static void LogElasticLocalityErrors(this ILogger logger, string errors, Exception exception = null)
            => ElasticLocalityErrors(logger, errors, exception);
                
         public static void LogElasticAccommodationErrors(this ILogger logger, string errors, Exception exception = null)
            => ElasticAccommodationErrors(logger, errors, exception);
    
    
        
        private static readonly Action<ILogger, int, Exception> ElasticCountryAdded;
        
        private static readonly Action<ILogger, int, Exception> ElasticLocalityAdded;
        
        private static readonly Action<ILogger, int, Exception> ElasticAccommodationAdded;
        
        private static readonly Action<ILogger, int, Exception> ElasticCountryUpdated;
        
        private static readonly Action<ILogger, int, Exception> ElasticLocalityUpdated;
        
        private static readonly Action<ILogger, int, Exception> ElasticAccommodationUpdated;
        
        private static readonly Action<ILogger, int, Exception> ElasticCountryDeleted;
        
        private static readonly Action<ILogger, int, Exception> ElasticLocalityDeleted;
        
        private static readonly Action<ILogger, int, Exception> ElasticAccommodationDeleted;
        
        private static readonly Action<ILogger, string, Exception> ElasticCountryErrors;
        
        private static readonly Action<ILogger, string, Exception> ElasticLocalityErrors;
        
        private static readonly Action<ILogger, string, Exception> ElasticAccommodationErrors;
    }
}