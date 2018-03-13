using Marketo.Require;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Marketo
{
    internal static class errors
    {
        enum errorCodes
        {
            TOKEN_EMPTY = 600,
            TOKEN_INVALID = 601,
            TOKEN_EXPIRED = 602,
            ACCESS_DENIED = 603,
            TIMED_OUT = 604,
            HTTP_NOT_SUPPORTED = 605,
            RATE_LIMIT_REACHED = 606,
            QUOTA_REACHED = 607,
            API_UNAVAILBLE = 608,
            INVALID_JSON = 609,
            RESOURCE_NOT_FOUND = 610,
            SYSTEM_ERROR = 611,
            INVALID_CONTENT_TYPE = 612,

            INVALID_VALUE = 1001,
            MISSING_REQUIRED_VALUE = 1002,
            INVALID_DATA = 1003,
            LEAD_NOT_FOUND = 1004,
            LEAD_ALREADY_EXISTS = 1005,
            FIELD_NOT_FOUND = 1006,
            MULTIPLE_MATCHING_LEADS = 1007,
            PARTITION_ACCESS_DENIED = 1008,
            PARTITION_UNSPECIFIED = 1009,
            PARTITION_UPDATE_NOT_ALLOWED = 1010,
            FIELD_UNSUPPORTED = 1011,
            INVALID_COOKIE = 1012,
            OBJECT_NOT_FOUND = 1013,
            OBJECT_CREATE_FAILED = 1014
        };

        static bool hasPlatformErrorCode(Exception err, int code)
        {
            return isPlatformError(err, out JArray errors) &&
                errors.FirstOrDefault(x => (string)x["code"] == code.ToString()) != null;
        }

        static bool isPlatformError(Exception err, out JArray errors)
        {
            errors = null;
            JObject res;
            if (!(err is RestlerOperationException) || (res = ((RestlerOperationException)err).Content as JObject) == null)
                return false;
            var val = res["requestId"] != null &&
                res["errors"] != null && res["errors"] is JArray &&
                ((JArray)res["errors"]).Count > 0;
            if (val)
                errors = (JArray)res["errors"];
            return val;
        }

        public static bool isNetworkError(Exception err)
        {
            return false;
            //var code = err["code"] || '';
            //return retryableNetworkErrorCodes.indexOf(code) > -1 || (err instanceof TimeoutError);
        }

        public static bool isServerError(Exception err)
        {
            //return (err is Http5XXError) ||
            return hasPlatformErrorCode(err, (int)errorCodes.TIMED_OUT) ||
                hasPlatformErrorCode(err, (int)errorCodes.API_UNAVAILBLE) ||
                hasPlatformErrorCode(err, (int)errorCodes.SYSTEM_ERROR);
        }

        public static bool isExpiredToken(Exception err)
        {
            return hasPlatformErrorCode(err, (int)errorCodes.TOKEN_EXPIRED) ||
                hasPlatformErrorCode(err, (int)errorCodes.TOKEN_INVALID);
        }

        public static bool isRateLimited(Exception err)
        {
            return hasPlatformErrorCode(err, (int)errorCodes.RATE_LIMIT_REACHED);
        }
    }
}
