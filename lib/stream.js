var _ = require('lodash'),
    Promise = require('bluebird'),
    Readable = require('stream').Readable,
    util = require('util');

function MarketoStream(client, object, method, args, options) {
  options = _.defaults(options || {}, { objectMode: true });
  Readable.call(this, options);
  this.client = client;
  if (this.client[object]) {
    this.object = this.client[object]
  } else {
    throw new Error('Not a valid object');
  }
  if (this.object[method]) {
    this.method = this.object[method]
  } else {
    throw new Error('Not a valid method');
  }
  this.args = args;
  this._ready = true;
}
util.inherits(MarketoStream, Readable);

MarketoStream.prototype._callMethod = function (promise) {
  if (promise) { return promise }
  return this.method.apply(this.object, this.args);
}


MarketoStream.prototype._pushNext = function() {
  var result = this._data.result;
  if (result) {
    if (this._resultIndex < result.length) {
      var record = result[this._resultIndex++]
      this.push(record);
      return true;
    }
  }
  return false;
};

MarketoStream.prototype.fetch = function (promise) {
  this._ready = false;
  var self = this;
  return this._callMethod(promise)
    .then(function(data) {
      if (data.result && data.result.length > 0) {
         self._data = data;
         self._resultIndex = 0;
         self._ready = true;
         self._pushNext();
       } else {
         self.push(null);
       }
    }).catch(function(err) {
      // emit an error and end the stream
      self.ended = true
      self.emit('error', err)
      self.push(null)
    })
}

MarketoStream.prototype._read = function () {
  if (!this._ready) { return; }
  if (!this._data) {
    this.fetch()
  } else if (this._pushNext()) {
    return;
  } else if (this._data.moreResult || this._data.nextPage) {
    this.fetch(this._data.nextPage())
  } else { // No data left in the batch and no more data from marketo, end the stream
    this.push(null);
  }
}

module.exports = MarketoStream;
