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
      if (data.result && data.result.length > 0) {
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

MarketoStream.prototype._read = function() {
  if (!this._ready) {
    return;
  }

  if (this._data.result) { // if there's still data in the current batch, push it to the stream
    if (this._pushNext()) {
      return;
    } else if (this._data.moreResult || this._data.nextPageToken) {
      this._setData(this._data.nextPage())
    } else { // No data left in the batch and no more data from marketo, end the stream
      this.push(null);
    }
  } else {
    this.push(null); // no data, end the stream
  }
};

MarketoStream.prototype.endMarketoStream = function() {
  this.push(null);
}

module.exports = MarketoStream;
