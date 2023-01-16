var _ = require('lodash'),
  Promise = require('bluebird'),
  util = require('../util'),
  Retry = require('../retry'),
  BulkFileStream = require('../BulkFileStream'),
  log = util.logger();

function BulkLeadExtract(marketo, connection) {
  this._marketo = marketo;
  this._connection = connection;
  this._retry = new Retry({
    maxRetries: 35,
    initialDelay: 45000,
    maxDelay: 120000,
  });
}

BulkLeadExtract.prototype = {
  create: function (fields, filter, options) {
    if (!_.isArray(fields)) {
      var msg = 'fields needs to be an Array';
      log.error(msg);
      return Promise.reject(msg);
    }
    var path = util.createBulkPath('leads', 'export', 'create.json');
    options = _.extend({}, options, {
      fields: fields,
      filter: filter,
    });
    return this._connection.post(path, options, { _method: 'POST' });
  },
  enqueue: function (exportId, options) {
    var path = util.createBulkPath('leads', 'export', exportId, 'enqueue.json');
    options = _.extend({}, options, {
      _method: 'POST',
    });
    return this._connection.post(path, { data: options });
  },
  status: function (exportId, options) {
    var path = util.createBulkPath('leads', 'export', exportId, 'status.json');
    options = _.extend({}, options, {
      _method: 'GET',
    });
    return this._connection.get(path, { data: options });
  },
  statusTillCompleted: function (exportId, options) {
    var self = this;
    var requestFn = function () {
      return new Promise(function (resolve, reject) {
        self
          .status(exportId)
          .then(function (data) {
            if (!data.success) {
              var msg = data.errors[0].message;
              log.error(msg);
              return reject(msg);
            }
            if (
              data.result[0].status == 'Queued' ||
              data.result[0].status == 'Processing'
            ) {
              return reject({
                requestId: data.requestId,
                errors: [
                  {
                    code: '0',
                    message: `${data.result[0].status} even after max retries`,
                  },
                ],
              });
            }
            resolve(data);
          })
          .catch(reject);
      });
    };
    return this._retry.start(requestFn, this);
  },
  cancel: function (exportId, options) {
    var path = util.createBulkPath('leads', 'export', exportId, 'cancel.json');
    options = _.extend({}, options, {
      _method: 'POST',
    });
    return this._connection.post(path, { data: options });
  },
  get: function (fields, filter, options) {
    var self = this;
    return new Promise(function (resolve, reject) {
      return self
        .create(fields, filter, options)
        .then(function (data) {
          if (!data.success) {
            var msg = data.errors[0].message;
            log.error(msg);
            return reject(msg);
          }
          let exportId = data.result[0].exportId;
          self
            .enqueue(exportId)
            .then(function (data) {
              if (!data.success) {
                var msg = data.errors[0].message;
                log.error(msg);
                return reject(msg);
              }
              self.statusTillCompleted(exportId).then(resolve).catch(reject);
            })
            .catch(function (err) {
              self
                .cancel(exportId)
                .then(function (data) {
                  return reject(err);
                })
                .catch(reject);
            });
        })
        .catch(reject);
    });
  },
  // FILE
  file: function (exportId, options) {
    var path = util.createBulkPath('leads', 'export', exportId, 'file.json');
    options = _.extend({}, options, {
      _method: 'GET',
    });
    return this._connection.get(path, { data: options });
  },
  fileStream: async function (exportId, fileSize, rangeSize) {
    var path = util.createBulkPath('leads', 'export', exportId, 'file.json');

    return new BulkFileStream(this._connection, path, fileSize, rangeSize);
  },
};

module.exports = BulkLeadExtract;
