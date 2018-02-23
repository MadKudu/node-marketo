var _ = require('lodash'),
  Promise = require('bluebird'),
  util = require('../util'),
  Retry = require('../retry'),
  MarketoStream = require('../stream'),
  log = util.logger();

// https://github.com/MadKudu/node-marketo
function BulkLeadExtract(marketo, connection) {
  this._marketo = marketo;
  this._connection = connection;
  this._retry = new Retry({ maxRetries: 50, initialDelay: 5000, maxDelay: 10000 });
}

BulkLeadExtract.prototype = {
  create: function (fields, filter, options) {
    if (!_.isArray(fields)) {
      var msg = 'fields needs to be an Array';
      log.error(msg);
      return Promise.reject(msg);
    }
    var path = '/../bulk' + util.createPath('leads', 'export', 'create.json');
    options = _.extend({}, options, {
      fields: fields,
      filter: filter,
      _method: 'POST'
    });
    return this._connection.post(path, { data: JSON.stringify(options), headers: { 'Content-Type': 'application/json' } });
  },
  enqueue: function (exportId, options) {
    var path = '/../bulk' + util.createPath('leads', 'export', exportId, 'enqueue.json');
    options = _.extend({}, options, {
      _method: 'POST'
    });
    return this._connection.post(path, { data: options });
  },
  status: function (exportId, options) {
    var path = '/../bulk' + util.createPath('leads', 'export', exportId, 'status.json');
    options = _.extend({}, options, {
      _method: 'GET'
    });
    return this._connection.get(path, { data: options });
  },
  statusTilComplete: function (exportId, options) {
    var self = this, defer = Promise.defer();
    var requestFn = function () {
      self.status(exportId).then(function (data) {
        if (!data.success) {
          var msg = data.errors[0].message;
          log.error(msg);
          return defer.reject(msg);
        }
        if (data.result[0].status == 'Queued' || data.result[0].status == 'Processing') {
          return defer.reject({
            requestId: data.requestId,
            errors: [{ code: '606' }]
          });
        }
        defer.resolve(data);
      }).catch(function (err) {
        defer.reject(err);
      });
      return defer.promise;
    };
    return this._retry.start(requestFn, this);
  },
  cancel: function (exportId, options) {
    var path = '/../bulk' + util.createPath('leads', 'export', exportId, 'cancel.json');
    options = _.extend({}, options, {
      _method: 'POST'
    });
    return this._connection.post(path, { data: options });
  },
  get: function (fields, filter) {
    var self = this, defer = Promise.defer();
    self.create(fields, filter).then(function (data) {
      if (!data.success) {
        var msg = data.errors[0].message;
        log.error(msg);
        return defer.reject(msg);
      }
      let exportId = data.result[0].exportId;
      self.enqueue(exportId).then(function (data) {
        if (!data.success) {
          var msg = data.errors[0].message;
          log.error(msg);
          return defer.reject(msg);
        }
        self.statusTilComplete(exportId).then(function (data) {
          return defer.resolve(data);
        }).catch(function (err) {
          return defer.reject(err);
        });
      }).catch(function (err) {
        self.cancel(exportId).then(function (data) {
          return defer.reject(err);
        }).catch(function (err) {
          return defer.reject(err);
        });
      });
    }).catch(function (err) {
      return defer.reject(err);
    });
    return defer.promise;
  },
  // FILE
  file: function (exportId, options) {
    var path = '/../bulk' + util.createPath('leads', 'export', exportId, 'file.json');
    options = _.extend({}, options, {
      _method: 'GET'
    });
    return this._connection.get(path, { data: options });
  },
  fileStream: function (exportId, options) {
    return new MarketoStream(this.file(exportId, options).then(function (data) {
      return { result: [data] };
    }));
  },
};

module.exports = BulkLeadExtract;