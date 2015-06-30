var _ = require('lodash'),
    Promise = require('bluebird'),
    util = require('../util'),
    log = util.logger();

var LISTS = util.createPath('lists.json');

function List(marketo, connection) {
  this._marketo = marketo;
  this._connection = connection;
}

List.prototype = {
  find: function(options) {
    var arrayFields = ['id', 'name', 'programName', 'workspaceName'];
    options = _.extend({}, options, {
      _method: 'GET'
    });
    options = util.arrayToCSV(options, arrayFields);
    return this._connection.post(LISTS, {data: options});
  },

  addLeadsToList: function(listId, input) {
    if (!_.isArray(input) && !_.isEmpty(input)) {
      var msg = 'input must be an array of lead ids';
      log.error(msg);
      return Promise.reject(msg);
    }

    var path = util.createPath('lists', listId, 'leads.json');

    input = _.compact(_.map(input, function(id) {
      if (_.isString(id) || _.isNumber(id)) {
        return {id: id};
      }

      if (_.isObject(id) && _.has(id, 'id')) {
        return _.pick(id, 'id');
      }
    }));

    return this._connection.postJson(path, {input: input});

  },

  removeLeadsFromList: function(listId, leadIds) {
    if (!_.isArray(leadIds) && !_.isEmpty(leadIds)) {
      var msg = 'input must be an array of lead ids';
      log.error(msg);
      return Promise.reject(msg);
    }

    var path = util.createPath('lists', listId, 'leads.json');
    leadIds = [].concat(leadIds); // ensure it's an array
    return this._connection.del(path + "?id=" + leadIds.join('&id='), {headers: {"Content-Type": "application/json"}});
  },

  addEmailsToList: function(emails, listId, options) {
    options = options || {};

    emails = _.map(emails, function(email) {
      return {email: email};
    });

    return this._marketo.lead.createOrUpdate(emails, {lookupField: 'email'})
      .then(_.bind(function(data) {
        return this.addLeadsToList(listId, data.result);
      }, this));
  },

  getLeads: function(listId, options) {
    var path = util.createPath('list', listId, 'leads.json');
    options = _.extend({}, options, {
      listId: listId,
      _method: 'GET'
    });
    return this._connection.post(path, {data: options});
  },

  isMember: function(listId, leadIds) {
    var path = util.createPath('lists', listId, 'leads', 'ismember.json');
    leadIds = [].concat(leadIds); // ensure it's an array
    return this._connection.get(path + "?id=" + leadIds.join('&id='));
  },

  // convenience function to find a single list by id
  byId: function(listId) {
    return this.find({id: [listId]});
  }
};

module.exports = List;
