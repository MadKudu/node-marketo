var _ = require('lodash'),
    Promise = require('bluebird'),
    util = require('../util'),
    log = util.logger();

var LEADS = util.createPath('leads.json');

function Lead(marketo, connection) {
  this._marketo = marketo;
  this._connection = connection;
}

Lead.prototype = {
  byId: function(id, options) {
    var path = util.createPath('lead', id + '.json');
    options = util.formatOptions(options, 'fields');

    return this._connection.get(path, {query: options});
  },

  find: function(filterType, filterValues, options) {
    if (!_.isArray(filterValues)) {
      var msg = 'filterValues needs to be an Array';
      log.error(msg);
      return Promise.reject(msg);
    }

    options = _.extend({}, options, {
      filterType: filterType,
      filterValues: filterValues,
      _method: 'GET'
    });
    options = util.formatOptions(options);

    return this._connection.post(LEADS, {data: options});
  },

  createOrUpdate: function(input, options) {
    if (!_.isArray(input) && !_.isEmpty(input)) {
      var msg = 'input must be an array of leads';
      log.error(msg);
      return Promise.reject(msg);
    }

    var data = _.extend({}, options, {
      input: input
    });

    return this._connection.postJson(LEADS, data)
      .spread(function(data, resp) {
        if (data.success) {
          return [data, resp];
        } else {
          log.warn('Cannot create/update lead: ', data);
          return Promise.reject('Cannot get lead(s) from input: ' + JSON.stringify(input));
        }
      });
  }
};

module.exports = Lead;
