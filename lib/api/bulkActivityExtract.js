var _ = require('lodash'),
  Promise = require('bluebird'),
  util = require('../util'),
  Retry = require('../retry'),
  BulkFileStream = require('../BulkFileStream'),
  log = util.logger();

function BulkActivityExtract(marketo, connection) {
  this._marketo = marketo;
  this._connection = connection;
  this._retry = new Retry({ maxRetries: 10, initialDelay: 30000, maxDelay: 60000 });
}

BulkActivityExtract.prototype = {
  create: function (filter, options) {
    var path = util.createBulkPath('activities', 'export', 'create.json');
    options = _.extend({}, options, {
      filter: filter,
    });
    return this._connection.postJson(path, options, { _method: 'POST' });
  },
  enqueue: function (exportId, options) {
    var path = util.createBulkPath('activities', 'export', exportId, 'enqueue.json');
    options = _.extend({}, options, {
      _method: 'POST'
    });
    return this._connection.post(path, { data: options });
  },
  status: function (exportId, options) {
    var path = util.createBulkPath('activities', 'export', exportId, 'status.json');
    options = _.extend({}, options, {
      _method: 'GET'
    });
    return this._connection.get(path, { data: options });
  },
  statusTillCompleted: function (exportId, options) {
    var self = this
    var requestFn = function () {
      return new Promise(function (resolve, reject) {
        self.status(exportId).then(function (data) {
          if (!data.success) {
            var msg = data.errors[0].message;
            log.error(msg);
            return reject(msg);
          }
          if (data.result[0].status == 'Queued' || data.result[0].status == 'Processing') {
            return reject({
              requestId: data.requestId,
              errors: [{ code: '606' }]
            });
          }
          resolve(data);
        }).catch(reject);
      })
    };
    return this._retry.start(requestFn, this);
  },
  cancel: function (exportId, options) {
    var path = util.createBulkPath('activities', 'export', exportId, 'cancel.json');
    options = _.extend({}, options, {
      _method: 'POST'
    });
    return this._connection.post(path, { data: options });
  },
  get: function (filter, options) {
    var self = this;
    return new Promise(function (resolve, reject) {
      self.create(filter, options).then(function (data) {
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
            return reject(msg);
          }
          self.statusTillCompleted(exportId)
            .then(resolve)
            .catch(reject);
        }).catch(function (err) {
          self.cancel(exportId).then(function () {
            return reject(err);
          }).catch(reject);
        });
      }).catch(reject);
    })
  },
  // FILE
  file: function (exportId, options) {
    var path = util.createBulkPath('activities', 'export', exportId, 'file.json');
    options = _.extend({}, options, {
      _method: 'GET'
    });
    return this._connection.get(path, { data: options });
  },
  fileStream: async function (exportId, fileSize, rangeSize) {
    var path = util.createBulkPath('activities', 'export', exportId, 'file.json');
    
    return new BulkFileStream(this._connection, path, fileSize, rangeSize);
  },
};

module.exports = BulkActivityExtract;
