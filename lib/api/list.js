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
  }
};

module.exports = List;
