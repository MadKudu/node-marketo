var _ = require('lodash'),
    Promise = require('bluebird'),
    util = require('../util'),
    log = util.logger();

function Campaign(marketo, connection) {
  this._marketo = marketo;
  this._connection = connection;
}

Campaign.prototype = {
  trigger: function(id, input, options) {
    if (!_.isArray(input) && !_.isEmpty(input)) {
      var msg = 'input must be an array of leads';
      log.error(msg);
      return Promise.reject(msg);
    }

    var path = util.createPath('campaigns', id, 'trigger.json');
    var data = {input: {leads: input}};
    data.input = _.extend(data.input, {
        tokens: options
    });

    return this._connection.postJson(path, data)
      .then(function(data) {
        if (data.success) {
          return data;
        } else {
          log.warn('Cannot trigger campaign: ', data);
          return Promise.reject('Cannot trigger campaign ' + id + ': ' + JSON.stringify(input));
        }
      });
  }
};

module.exports = Campaign;
