
var _ = require('lodash'),
  Promise = require('bluebird'),
  util = require('../util'),
  Retry = require('../retry'),
  MarketoStream = require('../stream'),
  log = util.logger();

function BulkExportCustomObjects(marketo, connection) {
  this._marketo = marketo;
  this._connection = connection;
  this._retry = new Retry({ maxRetries: 10, initialDelay: 30000, maxDelay: 60000 });
}


BulkExportCustomObjects.prototype = {
  create: function ( co_id, fields, filter, options) {
    
    if (!_.isString(co_id)) {
      var msg = 'co_id needs to be an String';
      log.error(msg);
      return Promise.reject(msg);
    }
    
    if (!_.isArray(fields)) {
      var msg = 'fields needs to be an Array';
      log.error(msg);
      return Promise.reject(msg);
    }
   
    var path = util.createBulkPath('customobjects', co_id, 'export', 'create.json');

    var columns = {};
    for( var i = 0; i < fields.length; i++ ){
      key = fields[i];
      columns[key] = key;
    }
    
    options = _.extend({}, options, {
      fields: fields,
      filter: filter,
      columnHeaderNames : columns
    });

    return this._connection.postJson(path, options, { _method: 'POST' });
  },
  enqueue: function (co_id, exportId, options) {
    var path = util.createBulkPath('customobjects', co_id, 'export', exportId, 'enqueue.json');
    options = _.extend({}, options, {
      _method: 'POST'
    });
    return this._connection.post(path, { data: options });
  },
  status: function (co_id, exportId, options) {
    var path = util.createBulkPath('customobjects', co_id, 'export', exportId, 'status.json');
    options = _.extend({}, options, {
      _method: 'GET'
    });
    return this._connection.get(path, { data: options });
  },
  statusTilCompleted: function (co_id, exportId, options) {
    var self = this;
    var requestFn = function () {
      return new Promise(function (resolve, reject) {
        self.status(co_id, exportId).then(function (data) {
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
      });
    };
    return this._retry.start(requestFn, this);
  },
  cancel: function (co_id, exportId, options) {
    var path = util.createBulkPath('customobjects', co_id, 'export', exportId, 'cancel.json');
    options = _.extend({}, options, {
      _method: 'POST'
    });
    return this._connection.post(path, { data: options });
  },
  get: function (co_id, fields, filter, options) {
    var self = this;
    return new Promise(function (resolve, reject) {
      return self.create(co_id, fields, filter, options).then(function (data) {
        if (!data.success) {
          var msg = data.errors[0].message;
          log.error(msg);
          return reject(msg);
        }
        let exportId = data.result[0].exportId;
        self.enqueue(co_id, exportId).then(function (data) {
          if (!data.success) {
            var msg = data.errors[0].message;
            log.error(msg);
            return reject(msg);
          }
          self.statusTilCompleted(co_id, exportId)
            .then(resolve)
            .catch(reject);
        }).catch(function (err) {
          self.cancel(co_id, exportId).then(function (data) {
            return reject(err);
          }).catch(reject);
        });
      }).catch(reject);
    })

  },
  // FILE
  file: function (co_id, exportId, options) {
    var path = util.createBulkPath('customobjects', co_id, 'export', exportId, 'file.json');
    options = _.extend({}, options, {
      _method: 'GET'
    });
    return this._connection.get(path, { data: options });
  },
  fileStream: function (co_id, exportId, options) {
    return new MarketoStream(this.file(co_id, exportId, options).then(function (data) {
      return { result: [data] };
    }));
  },
};

module.exports = BulkExportCustomObjects;