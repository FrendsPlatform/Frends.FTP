using Frends.FTP.UploadFiles.TaskConfiguration;
using Frends.FTP.UploadFiles.TaskResult;
using System;

namespace Frends.FTP.UploadFiles.Helpers
{
    internal static class ErrorHandler
    {
        /// <summary>
        /// Converts an exception into a failed Result object or rethrows based on task options.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="options">Task options that control whether failures are returned as a Result object or thrown.</param>
        /// <param name="transferResult">Optional transfer result to include in the returned failed Result.</param>
        /// <returns>A failed Result object when the exception is handled instead of rethrown.</returns>
        internal static Result Handle(this Exception exception, Options options, FileTransferResult transferResult = null)
        {
            if (exception is OperationCanceledException) throw exception;
            if (options.ThrowErrorOnFailure) ThrowBaseException(exception, options.ErrorMessageOnFailure);

            return ReturnResult(exception, options.ErrorMessageOnFailure, transferResult);
        }

        private static void ThrowBaseException(Exception exception, string customMessage)
        {
            if (string.IsNullOrEmpty(customMessage))
                throw new Exception(exception.Message, exception);

            throw new Exception(customMessage, exception);
        }

        private static Result ReturnResult(Exception exception, string customMessage, FileTransferResult transferResult)
        {
            var errorMessage = string.IsNullOrEmpty(customMessage)
                ? exception.Message
                : $"{customMessage}: {exception.Message}";

            var error = new Error { Message = errorMessage, AdditionalInfo = exception };
            return new Result(transferResult ?? new FileTransferResult { Success = false, UserResultMessage = errorMessage }, error);
        }
    }
}
