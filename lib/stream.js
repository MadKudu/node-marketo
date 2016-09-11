var _ = require('lodash'),
    Promise = require('bluebird'),
    Readable = require('stream').Readable,
    util = require('util');

function MarketoStream(marketoResultPromise, options) {
  options = _.defaults(options || {}, {objectMode: true});
  Readable.call(this, options);
  this._setData(marketoResultPromise || Promise.resolve({}));
}
util.inherits(MarketoStream, Readable);

MarketoStream.prototype._setData = function(dataPromise) {
  this._ready = false;

  var self = this;
  this._dataPromise = dataPromise;
  return dataPromise.then(
    function(data) {
      if (data.result) {
        self._data = data;
        self._resultIndex = 0;
        self._ready = true;
        self._pushNext();
      } else {
        self.push(null);
      }
    },
    function(err) {
      self.emit('error', err);
      // end the stream
      self.push(null);
    });
};

MarketoStream.prototype._read = function() {

  if (!this._ready) {
    return;
  }

  if (this._data.result) {
    if (this._pushNext()) {
      return;
    } else if (this._data.nextPageToken) {
      this._setData(this._data.nextPage())
    } else {
      // No data left and no more data from marketo
      this.push(null);
    }
  } else {
    this.push(null);
  }
};

MarketoStream.prototype._pushNext = function() {
  var result;
  if (this._data.result) {
    result = this._data.result;
    if (this._resultIndex < result.length) {
      this.push(result[this._resultIndex++]);
      return true;
    }
  }

  return false;
};

MarketoStream.prototype.endMarketoStream = function() {
  this.push(null);
}

module.exports = MarketoStream;
