var _ = require('lodash'),
    rest = require("./restler/restler"),
    Promise = require('bluebird'),
    Retry = require('./retry'),
    errors = require('./errors'),
    getNextPageFn = require('./nextPageFn'),
    util = require('util'),
    helper = require('./util'),
    EventEmitter = require('events').EventEmitter,
    log = require('./util').logger(),
    restlerMethodArgCount = {
      get: 2,
      post: 2,
      put: 2,
      del: 2,
      head: 2,
      patch: 2,
      json: 3,
      postJson: 3,
      putJson: 3
    };


var OAUTH_TOKEN_PATH = '/oauth/token';

function Connection(options) {
  EventEmitter.call(this);
  this._options = options || {};
  this._tokenData = null;
  this._retry = new Retry(options.retry || {});
  this.apiCallCount = 0;

  // Convenient methods, matches that of restler's
  _.forEach(restlerMethodArgCount, (count, method) => {
    this[method] = function(var_args) {
      var args = Array.prototype.slice.call(arguments);
      while (args.length < count) {
        args.push(undefined);
      }
      args.unshift(method);
      return this._request.apply(this, args);
    };
  });
}

util.inherits(Connection, EventEmitter);

/**
 * This function is just a helper function that delegates everything to
 * restler, but returns a Promise instead of going with the existing event
 * based responses.
 */
Connection.prototype._request = function(method) {
  var args = Array.prototype.slice.call(arguments, 1),
      url = _.first(args);

  // If the url does not start with http(s)://, then assume that it's an
  // absolute path from options.endpoint
  if (!/^https?:\/\//i.test(url)) {
    args[0] = this._options.endpoint + url;
  }

  this.apiCallCount += 1;
  this.emit('apiCall', { apiCallCount: this.apiCallCount });
  log.debug('Request:', arguments);

  var requestFn = function requestFn(forceOAuth) {
    return this.getOAuthToken(forceOAuth)
      .then(function(token) {
        var defer = Promise.defer(),
            options = _.last(args) || {},
            nextPageFn = getNextPageFn(this, method, args);

        args.pop();
        options.headers = _.extend({}, options.headers, {
          'Authorization': 'Bearer ' + token.access_token
        });
        args.push(options);
        rest[method].apply(rest, args)
          .on('success', function(data, resp) {
            if (data.success === false && _.has(data, 'errors')) {
              log.debug('Request failed: ', data);
              defer.reject(new errors.HttpError(resp.statusCode, data));
            } else {
              if(helper.nextPageType(args[0]) === 'offset') {
                if(!_.isEmpty(data.result)) {
                  Object.defineProperty(data, 'nextPage', {
                    enumerable: false,
                    value: _.partial(nextPageFn)
                  });
                }
              } else {
                if (_.has(data, 'nextPageToken')) {
                  Object.defineProperty(data, 'nextPage', {
                    enumerable: false,
                    value: _.partial(nextPageFn, data.nextPageToken)
                  });
                }
              }
              
              defer.resolve(data);
            }
          })
          .on('error', function(err, resp) {
            defer.reject(err);
          })
          .on('fail', function(data, resp) {
            defer.reject(new errors.HttpError(resp.statusCode, data));
          })
          .on('timeout', function(ms) {
            defer.reject(new errors.TimeoutError(ms));
          });

          return defer.promise;
      }.bind(this), function(e) {
          throw new Error('Authentication (' + e.error + '): ' + e.error_description);
      });
  };

  return this._retry.start(requestFn, this);
};

Connection.prototype.getOAuthToken = function(force) {
  var requestFn, defer;
  force = force || false;

  if (force || this._tokenData == null) {
    defer = Promise.defer();

    requestFn = function() {
      var getOptions = {
        query: this._getTokenQuery(),
        timeout: this._options.timeout || 20000
      };
      rest.get(this._options.identity + OAUTH_TOKEN_PATH, getOptions)
        .on('success', function(data, resp) {
          log.debug('Got token: ', data);
          this._tokenData = data;
          defer.resolve(data);
        }.bind(this))
        .on('timeout', function(ms) {
          log.debug('Timeout');
          defer.reject(ms);
        })
        .on('fail', function(data, resp) {
          defer.reject(data);
        })
        .on('error', function(err){
          defer.reject(err);
        });
      return defer.promise;
    };
    return this._retry.start(requestFn, this);
  } else {
    log.debug('Using existing token: ', this._tokenData);
    return Promise.resolve(this._tokenData);
  }
};

Connection.prototype._getTokenQuery = function() {
  var queryOptions = {
    grant_type: 'client_credentials',
    client_id: this._options.clientId,
    client_secret: this._options.clientSecret
  };
  var partnerId = this._options.partnerId;
  if (partnerId) {
    queryOptions.partner_id = partnerId;
  }
  return queryOptions;
};

module.exports = Connection;
