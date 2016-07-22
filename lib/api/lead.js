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
      .then(function(data) {
        if (data.success) {
          return data;
        } else {
          log.warn('Cannot create/update lead: ', data);
          return Promise.reject('Cannot get lead(s) from input: ' + JSON.stringify(input));
        }
      });
  },

  associateLead: function(leadId, cookieId) {
    var path = util.createPath('leads', leadId, 'associate.json');
    return this._connection.postJson(path, {}, {query: {cookie: cookieId}});
  },

  describe: function() {
    var path = util.createPath('leads', 'describe.json');
    return this._connection.get(path);
  },

  partitions: function() {
    var path = util.createPath('leads', 'partitions.json');
    return this._connection.get(path);
  },

  mergeLead: function(winningLead, losingLeads, options) {
    if (!_.isArray(losingLeads) && !_.isEmpty(losingLeads)) {
      var msg = 'input must be an array of lead ids';
      log.error(msg);
      return Promise.reject(msg);
    }

    var path = util.createPath('leads', winningLead, 'merge.json');
    return this._connection.postJson(path, {data: options}, {query: {leadIds: losingLeads.join()}});
  },

  getPageToken: function(sinceDatetime,options) {
    var path = util.createPath('activities','pagingtoken.json');
    options = _.extend({}, options, {
      sinceDatetime: sinceDatetime,
      access_token: '801ca234-6103-4362-8c2e-9062fff73295:sj',
      _method: 'GET'
    });
    options = util.formatOptions(options);
    return this._connection.post(path, {data: options});
  },

  getActivities: function(leadId, activityId,pageToken,options) {
    var path = util.createPath('activities.json');
    options = _.extend({}, options, {
      leadIds: leadId.join(),
      access_token: '801ca234-6103-4362-8c2e-9062fff73295:sj',
      activityTypeIds: activityId.join(),
      nextPageToken: pageToken,
      _method: 'GET'
    });
    options = util.formatOptions(options);
    return this._connection.post(path, {data: options});
  }
};

module.exports = Lead;
