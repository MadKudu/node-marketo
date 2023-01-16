var _ = require('lodash'),
  backoff = require('backoff'),
  Promise = require('bluebird'),
  errors = require('./errors'),
  util = require('./util'),
  log = util.logger();

function Retry(options) {
  this._options = options || {};
}

Retry.prototype = {
  defaultRetryCount: 5,
  defaultInitialDelay: 1000, // 1 second
  defaultMaxDelay: 20000,

  numRetries: function () {
    return this._options.maxRetries || this.defaultRetryCount;
  },

  initialDelay: function () {
    return this._options.initialDelay || this.defaultInitialDelay;
  },

  maxDelay: function () {
    return this._options.maxDelay || this.defaultMaxDelay;
  },

  // Starts the retry loop, you can pass in an optional context for the
  // passed in function to be in.
  start: function (fetchFn, opt_context) {
    var self = this,
      def = Promise.defer(),
      expBackoff = this._newBackoff(),
      forceOAuth = false,
      lastError;

    if (_.isObject(opt_context)) {
      fetchFn = _.bind(fetchFn, opt_context);
    }

    executeFn();
    expBackoff.on('ready', function (number, delay) {
      log.debug('Requesting for url', { number: number, delay: delay });
      executeFn();
    });

    expBackoff.on('fail', function () {
      def.reject(lastError);
    });

    function executeFn() {
      fetchFn(forceOAuth)
        .then(function (result) {
          def.resolve(result);
        })
        .catch(function (err) {
          lastError = err;
          if (self.retryableError(err)) {
            if (errors.isRateLimited(err)) {
              // HACK: there's no way to custom set the next delay in node-backoff
              // so we'll have to do a manual timeout here
              setTimeout(function () {
                expBackoff.backoff();
              }, self.maxDelay());
            } else {
              if (errors.isExpiredToken(err)) {
                forceOAuth = true;
              }
              expBackoff.backoff();
            }
          } else {
            def.reject(err);
          }
        });
    }

    return def.promise;
  },

  retryableError: function (err) {
    return (
      errors.isAwaitable(err) ||
      errors.isNetworkError(err) ||
      errors.isExpiredToken(err) ||
      errors.isServerError(err) ||
      errors.isRateLimited(err) ||
      false
    );
  },

  _newBackoff: function () {
    var expBackoff = backoff.exponential({
      randomisationFactor: 0,
      initialDelay: this.initialDelay(),
      maxDelay: this.maxDelay(),
    });
    expBackoff.failAfter(this.numRetries());

    return expBackoff;
  },
};

module.exports = Retry;
