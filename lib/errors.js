var _ = require('lodash'),
    util = require('util'),
    retryableNetworkErrorCodes,

    // Marketo error codes
    // http://developers.marketo.com/documentation/rest/error-codes/
    mkCode = {
      TOKEN_EMPTY: '600',
      TOKEN_INVALID: '601',
      TOKEN_EXPIRED: '602',
      ACCESS_DENIED: '603',
      TIMED_OUT: '604',
      HTTP_NOT_SUPPORTED: '605',
      RATE_LIMIT_REACHED: '606',
      QUOTA_REACHED: '607',
      API_UNAVAILBLE: '608',
      INVALID_JSON: '609',
      RESOURCE_NOT_FOUND: '610',
      SYSTEM_ERROR: '611',
      INVALID_CONTENT_TYPE: '612',

      INVALID_VALUE: '1001',
      MISSING_REQUIRED_VALUE: '1002',
      INVALID_DATA: '1003',
      LEAD_NOT_FOUND: '1004',
      LEAD_ALREADY_EXISTS: '1005',
      FIELD_NOT_FOUND: '1006',
      MULTIPLE_MATCHING_LEADS: '1007',
      PARTITION_ACCESS_DENIED: '1008',
      PARTITION_UNSPECIFIED: '1009',
      PARTITION_UPDATE_NOT_ALLOWED: '1010',
      FIELD_UNSUPPORTED: '1011',
      INVALID_COOKIE: '1012',
      OBJECT_NOT_FOUND: '1013',
      OBJECT_CREATE_FAILED: '1014'
    };

// These error codes were pulled from
// https://github.com/joyent/node/blob/857975d5e7e0d7bf38577db0478d9e5ede79922e/deps/uv/include/uv-errno.h
// Is there a better way to do this?
retryableNetworkErrorCodes = [
  'EADDRINFO',
  'EADDRNOTAVAIL',
  'ECONNRESET',
  'ENOTFOUND',
  'ECONNABORTED',
  'EPROTO',
  'ECONNREFUSED',
  'EHOSTUNREACH',
  'ENETDOWN',
  'ENETUNREACH',
  'ENONET',
  'ENOTCONN',
  'ENOTSOCK',
  'ETIMEDOUT'
];

function Http5XXError(msg) {
  Error.call(this);
  this.message = msg;
  this.code = '5XX';
}
util.inherits(Http5XXError, Error);

function Http4XXError(msg) {
  Error.call(this);
  this.message = msg;
  this.code = '4XX';
}
util.inherits(Http4XXError, Error);

function TimeoutError(timeout) {
  Error.call(this, 'Timed out after ' + timeout + ' ms.');
}
util.inherits(TimeoutError, Error);


function hasMarketoCode(err, code) {
  return isMarketoError(err) &&
         !!_.find(err.errors, {code: code});
}

function isMarketoError(err) {
  var val = _.has(err, 'requestId') &&
            _.isArray(err.errors) &&
            err.errors.length > 0;
  return val;
}

module.exports = {
  Http5XXError: Http5XXError,
  Http4XXError: Http4XXError,
  isMarketoError: isMarketoError,
  hasMarketoCode: hasMarketoCode,
  marketoErrorCodes: mkCode,

  isNetworkError: function(err) {
    var code = err.code || '';
    return retryableNetworkErrorCodes.indexOf(code) > -1 ||
           (err instanceof TimeoutError);
  },

  isServerError: function(err) {
    return (err instanceof Http5XXError) ||
           hasMarketoCode(err, mkCode.TIMED_OUT) ||
           hasMarketoCode(err, mkCode.API_UNAVAILBLE) ||
           hasMarketoCode(err, mkCode.SYSTEM_ERROR);
  },

  isExpiredToken: function(err) {
    return hasMarketoCode(err, mkCode.TOKEN_EXPIRED) ||
	   hasMarketoCode(err, mkCode.TOKEN_INVALID);
  },

  isRateLimited: function(err) {
    return hasMarketoCode(err, mkCode.RATE_LIMIT_REACHED);
  }
};
