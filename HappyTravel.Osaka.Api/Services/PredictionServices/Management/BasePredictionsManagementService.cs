using System;
using System.Linq;
using System.Text;
using Nest;
using Result = CSharpFunctionalExtensions.Result;

namespace HappyTravel.Osaka.Api.Services.PredictionServices.Management
{
    public class BasePredictionsManagementService
    {
        internal Result HandleResponse(BulkResponse response, Action<string> logErrorsFunc, Action<int> logSuccessFunc)
        {
            if (!response.IsValid)
                return Result.Failure(response.DebugInformation);

            var errors = GetErrors(response);
        
            if (errors.Any())
                logErrorsFunc(errors);

            var numberOfSuccessfullyUploaded = response.Items.Select(i => i.Id).Except(response.ItemsWithErrors.Select(i => i.Id)).Count();

            logSuccessFunc(numberOfSuccessfullyUploaded);
            
            return Result.Success();
        }
        
        
        private string GetErrors(BulkResponse response)
        {
            var stringBuilder = new StringBuilder();
            if (response.ItemsWithErrors.Any())
            {
                foreach (var item in response.ItemsWithErrors)
                {
                    stringBuilder.AppendLine($"{nameof(item.Id)}: '{item.Id}', {nameof(item.Error)}: '{item.Error}'");
                }
            }

            return stringBuilder.ToString();
        }
    }
}