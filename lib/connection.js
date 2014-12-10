var _ = require('lodash'),
    rest = require('restler'),
    Promise = require('bluebird'),
    Retry = require('./retry')
    errors = require('./errors'),
    util = require('./util'),
    log = util.logger(),
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
  this._options = options || {};
  this._tokenData = null;
  this._retry = new Retry(options.retry || {});

  // Convenient methods, matches that of restler's
  _.each(restlerMethodArgCount, function(count, method) {
    this[method] = function(var_args) {
      var args = Array.prototype.slice.call(arguments);
      while (args.length < count) {
        args.push(undefined);
      }
      args.unshift(method);

      return this._request.apply(this, args);
    };
  }, this);
}

Connection.prototype = {
  /**
   * This function is just a helper function that delegates everything to
   * restler, but returns a Promise instead of going with the existing event
   * based resposnes.
   */
  _request: function(method) {
    var args = Array.prototype.slice.call(arguments, 1),
        url = _.first(args);

    // If the url does not start with http(s)://, then assume that it's an
    // absolute path from options.endpoint
    if (!/^https?:\/\//i.test(url)) {
      args[0] = this._options.endpoint + url;
    }

    log.debug('Request:', arguments);

    var requestFn = function requestFn(forceOAuth) {
      return this.getOAuthToken(forceOAuth)
        .then(function(token) {
          var defer = Promise.defer(),
              options = _.last(args) || {};
          args.pop();
          options.headers = _.extend({}, options.headers, {
            'Authorization': 'Bearer ' + token.access_token
          });
          args.push(options);

          rest[method].apply(rest, args)
            .on('success', function(data, resp) {
              if (data.success === false && _.has(data, 'errors')) {
                log.debug('Request failed: ', data);
                defer.reject(data);
              } else {
                defer.resolve(data);
              }
            })
            .on('error', function(err, resp) {
              defer.reject(err);
            })
            .on('fail', function(data, resp) {
              // We cannot hook into the 4XX and 5XX events because the fail event
              // appears to fire first. Both 4XX and 5XX errors trigger the 'fail'
              // event.
              var statusCode = Math.floor(resp.statusCode / 100);

              if (statusCode === 5) {
                defer.reject(new errors.Http5XXError(data));
              } else if (statusCode === 4) {
                defer.reject(new errors.Http4XXError(data));
              } else {
                defer.reject(data);
              }
            });

            return defer.promise;
        }, function(e) {
          log.info('Unable to authenticate: ', e[0]);
        });
    }

    return this._retry.start(requestFn, this);
  },

  getOAuthToken: function(force) {
    var requestFn;
    force = force || false;

    if (force || this._tokenData == null) {
      defer = Promise.defer();

      requestFn = function() {
        rest.get(this._options.identity + OAUTH_TOKEN_PATH, {query: this._getTokenQuery()})
          .on('success', function(data, resp) {
            log.debug('Got token: ', data);
            this._tokenData = data;
            defer.resolve(data);
          }.bind(this))
          .on('fail', function(data, resp) {
            defer.reject(data);
          });
        return defer.promise;
      };
      return this._retry.start(requestFn, this);
    } else {
      log.debug('Using existing token: ', this._tokenData);
      return Promise.resolve(this._tokenData);
    }
  },

  _getTokenQuery: function() {
    return {
      grant_type: 'client_credentials',
      client_id: this._options.clientId,
      client_secret: this._options.clientSecret
    };
  }
};

module.exports = Connection;
