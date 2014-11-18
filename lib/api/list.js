var _ = require('lodash'),
    Promise = require('bluebird'),
    util = require('../util');

var LISTS = util.createPath('lists.json');

function List(marketo, connection) {
  this._marketo = marketo;
  this._connection = connection;
}

List.prototype = {
  all: function() {
    return this._connection.get(LISTS);
  },

  addEmailsToList: function(emails, listId, options) {
    options = options || {};

    emails = _.map(emails, function(email) {
      return {email: email};
    });

    return this._marketo.lead.createOrUpdate(emails, {lookupField: 'email'})
      .spread(_.bind(function(data, resp) {
        var path = util.createPath('lists', listId, 'leads.json');
        var data = _.extend({}, options, {
          input: _.compact(_.map(data.result, function(r) {
                   if (r.id) {
                     return {id: r.id};
                   }
                 }))
        });

        return this._connection.postJson(path, data)
          .spread(function(data, resp) {
            if (data.success) {
              return [data, resp];
            } else {
              return Promise.reject('Cannot add to list: '
                                    + listId + ', emails: ' + emails);
            }
          });
      }, this));
  }
};

module.exports = List;
