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

// for the record, here's the format of the Marketo data in case of error:
// {
//   requestId: '15381#161d91d807a',
//   success: false,
//   errors: [ { code: '605', message: 'Request method \'POST\' not supported' } ]
//  }

function getFirstMessage(data) {
  return data && data.errors && data.errors[0] ? data.errors[0].message : 'Unknown Marketo error';
}

function isMarketoError(err) {
  return _.isArray(err.errors) && err.errors.length > 0;
}

function hasMarketoCode(err, code) {
  return isMarketoError(err) && !!_.find(err.errors, {code: code});
}

function isNetworkError(err) {
  var code = err.code || '';
  return retryableNetworkErrorCodes.indexOf(code) > -1 ||
   (err instanceof TimeoutError);
};

function isServerError(err) {
  return (Math.floor(err.code / 100) === 5) ||
   hasMarketoCode(err, mkCode.TIMED_OUT) ||
   hasMarketoCode(err, mkCode.API_UNAVAILBLE) ||
   hasMarketoCode(err, mkCode.SYSTEM_ERROR);
};

function isExpiredToken(err) {
  return hasMarketoCode(err, mkCode.TOKEN_EXPIRED) ||
   hasMarketoCode(err, mkCode.TOKEN_INVALID);
};

function isRateLimited(err) {
  return hasMarketoCode(err, mkCode.RATE_LIMIT_REACHED);
};

function isAwaitable(err) {
  return err?.errors?.[0]?.code === '0';
};

function HttpError(statusCode, data) {
  Error.call(this);
  this.name = 'HttpError'
  this.message = getFirstMessage(data);
  this.errors = data && data.errors;
  this.code = statusCode;
}
util.inherits(HttpError, Error);

function TimeoutError(timeout) {
  Error.call(this, 'Timed out after ' + timeout + ' ms.');
}
util.inherits(TimeoutError, Error);

module.exports = {
  HttpError: HttpError,
  isMarketoError: isMarketoError,
  hasMarketoCode: hasMarketoCode,
  marketoErrorCodes: mkCode,
  isNetworkError: isNetworkError,
  isServerError: isServerError,
  isExpiredToken: isExpiredToken,
  isRateLimited: isRateLimited,
  isAwaitable: isAwaitable,
};
