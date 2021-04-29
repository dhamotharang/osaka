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
            
            GetLocationsFromMapperOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(2003, "GetLocationsFromMapper"),
                $"INFORMATION | LocationsManagementService: {{message}}");
            
            CompleteUploadingLocationsOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(2004, "CompleteUploadingLocations"),
                $"INFORMATION | LocationsManagementService: {{message}}");
            
            UploadingErrorOccured = LoggerMessage.Define<string>(LogLevel.Critical,
                new EventId(2005, "UploadingError"),
                $"CRITICAL | LocationsManagementService: {{message}}");
            
            PredictionsQueryOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(2006, "PredictionsQuery"),
                $"INFORMATION | LocationsService: {{message}}");
            
            AddLocationsOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(2007, "AddLocations"),
                $"INFORMATION | LocationsManagementService: {{message}}");
            
            UpdateLocationsOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(2008, "UpdateLocations"),
                $"INFORMATION | LocationsManagementService: {{message}}");
            
            RemoveLocationsOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(2009, "RemoveLocations"),
                $"INFORMATION | LocationsManagementService: {{message}}");
            
        }
    
                
         public static void LogStartUploadingLocations(this ILogger logger, string message)
            => StartUploadingLocationsOccured(logger, message, null);
                
         public static void LogRemoveLocationsFromIndex(this ILogger logger, string message)
            => RemoveLocationsFromIndexOccured(logger, message, null);
                
         public static void LogGetLocationsFromMapper(this ILogger logger, string message)
            => GetLocationsFromMapperOccured(logger, message, null);
                
         public static void LogCompleteUploadingLocations(this ILogger logger, string message)
            => CompleteUploadingLocationsOccured(logger, message, null);
                
         public static void LogUploadingError(this ILogger logger, string message)
            => UploadingErrorOccured(logger, message, null);
                
         public static void LogPredictionsQuery(this ILogger logger, string message)
            => PredictionsQueryOccured(logger, message, null);
                
         public static void LogAddLocations(this ILogger logger, string message)
            => AddLocationsOccured(logger, message, null);
                
         public static void LogUpdateLocations(this ILogger logger, string message)
            => UpdateLocationsOccured(logger, message, null);
                
         public static void LogRemoveLocations(this ILogger logger, string message)
            => RemoveLocationsOccured(logger, message, null);
    
    
        
        private static readonly Action<ILogger, string, Exception> StartUploadingLocationsOccured;
        
        private static readonly Action<ILogger, string, Exception> RemoveLocationsFromIndexOccured;
        
        private static readonly Action<ILogger, string, Exception> GetLocationsFromMapperOccured;
        
        private static readonly Action<ILogger, string, Exception> CompleteUploadingLocationsOccured;
        
        private static readonly Action<ILogger, string, Exception> UploadingErrorOccured;
        
        private static readonly Action<ILogger, string, Exception> PredictionsQueryOccured;
        
        private static readonly Action<ILogger, string, Exception> AddLocationsOccured;
        
        private static readonly Action<ILogger, string, Exception> UpdateLocationsOccured;
        
        private static readonly Action<ILogger, string, Exception> RemoveLocationsOccured;
    }
}