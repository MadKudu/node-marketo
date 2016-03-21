var _ = require('lodash'),
    Promise = require('bluebird'),
    util = require('../util'),
    qs = require('querystring'),
    log = util.logger();

function Email(marketo, connection) {
  this._marketo = marketo;
  this._connection = connection;
}

Email.prototype = {
  updateEmailContentEditableText: function(emailId, htmlId, html, text, options) {
    var path = util.createAssetPath( 'email', emailId, 'content', '' + htmlId + '.json' );
    options = _.extend({}, options, {
      _method: 'POST'
    });
    return this._connection.post(path, {'data': { 'type':'Text','value':html,'textValue':text }});
  },
  approveEmail: function(emailId, options) {
    var path = util.createAssetPath( 'email', emailId, 'approveDraft.json' );
    options = _.extend({}, options, {
      _method: 'POST'
    });

    return this._connection.post(path, {data: options});
  },
  getEmailContent: function(emailId, options) {
    var path = util.createAssetPath( 'email', emailId, 'content.json' );
    options = _.extend({}, options, {
      _method: 'GET'
    });

    return this._connection.get(path, {data: options});
  },
  getEmails: function(options) {
    var path = util.createAssetPath( 'emails.json' );
    options = _.extend({}, options, {
      _method: 'GET'
    });
    return this._connection.get(path, {data: options});
  },
};

module.exports = Email;
