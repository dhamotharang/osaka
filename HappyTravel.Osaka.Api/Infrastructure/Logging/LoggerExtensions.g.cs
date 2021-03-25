using System;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Osaka.Api.Infrastructure.Logging
{
    public static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            StartUploadingLocationsOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(2001, "StartUploadingLocations"),
                $"INFORMATION | LocationsManagementService: {{message}}");
            
            RemoveLocationsFromIndexOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(2002, "RemoveLocationsFromIndex"),
                $"INFORMATION | LocationsManagementService: {{message}}");
            
            LocationsReceivedFromMapperOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(2003, "LocationsReceivedFromMapper"),
                $"INFORMATION | LocationsManagementService: {{message}}");
            
            LocationsUploadedToIndexOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(2004, "LocationsUploadedToIndex"),
                $"INFORMATION | LocationsManagementService: {{message}}");
            
            CompleteUploadingLocationsOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(2005, "CompleteUploadingLocations"),
                $"INFORMATION | LocationsManagementService: {{message}}");
            
            UploadingErrorOccured = LoggerMessage.Define<string>(LogLevel.Critical,
                new EventId(2006, "UploadingError"),
                $"CRITICAL | LocationsManagementService: {{message}}");
            
            PredictionsQueryOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(2007, "PredictionsQuery"),
                $"INFORMATION | LocationsService: {{message}}");
            
        }
    
                
         public static void LogStartUploadingLocations(this ILogger logger, string message)
            => StartUploadingLocationsOccured(logger, message, null);
                
         public static void LogRemoveLocationsFromIndex(this ILogger logger, string message)
            => RemoveLocationsFromIndexOccured(logger, message, null);
                
         public static void LogLocationsReceivedFromMapper(this ILogger logger, string message)
            => LocationsReceivedFromMapperOccured(logger, message, null);
                
         public static void LogLocationsUploadedToIndex(this ILogger logger, string message)
            => LocationsUploadedToIndexOccured(logger, message, null);
                
         public static void LogCompleteUploadingLocations(this ILogger logger, string message)
            => CompleteUploadingLocationsOccured(logger, message, null);
                
         public static void LogUploadingError(this ILogger logger, string message)
            => UploadingErrorOccured(logger, message, null);
                
         public static void LogPredictionsQuery(this ILogger logger, string message)
            => PredictionsQueryOccured(logger, message, null);
    
    
        
        private static readonly Action<ILogger, string, Exception> StartUploadingLocationsOccured;
        
        private static readonly Action<ILogger, string, Exception> RemoveLocationsFromIndexOccured;
        
        private static readonly Action<ILogger, string, Exception> LocationsReceivedFromMapperOccured;
        
        private static readonly Action<ILogger, string, Exception> LocationsUploadedToIndexOccured;
        
        private static readonly Action<ILogger, string, Exception> CompleteUploadingLocationsOccured;
        
        private static readonly Action<ILogger, string, Exception> UploadingErrorOccured;
        
        private static readonly Action<ILogger, string, Exception> PredictionsQueryOccured;
    }
}