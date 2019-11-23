var _ = require('lodash'),
    util = require('../util');

var SMARTLISTS = util.createAssetPath('smartLists.json');

function SmartList(marketo, connection) {
  this._marketo = marketo;
  this._connection = connection;
}

SmartList.prototype = {
  find: function(options) {
    var arrayFields = [];
    options = _.extend({}, options, {
      _method: 'GET',
    });
    options = util.arrayToCSV(options, arrayFields);
    return this._connection.post(SMARTLISTS, {data: options});
  },
  byId: function(smartListId, options) {
    var SMARTLISTS_ID = util.createAssetPath('smartList', `${smartListId}.json`);
    var arrayFields = [];
    options = _.extend({}, options, {
      _method: 'GET',
    });
    options = util.arrayToCSV(options, arrayFields);
    return this._connection.post(SMARTLISTS_ID, {data: options});
  }
};

module.exports = SmartList;