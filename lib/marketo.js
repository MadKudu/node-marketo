var _ = require('lodash'),
    util = require('util'),
    EventEmitter = require('events').EventEmitter,
    Activities = require('./api/activities'),
    Connection = require('./connection'),
    Campaign = require('./api/campaign'),
    Email = require('./api/email'),
    LandingPage = require('./api/landingPage'),
    Lead = require('./api/lead'),
    List = require('./api/list'),
    Stats = require('./api/stats'),
    BulkLeadExtract = require('./api/bulkLeadExtract'),
    BulkActivityExtract = require('./api/bulkActivityExtract'),
    MarketoStream = require('./stream');

function Marketo(options) {
  EventEmitter.call(this);
  var self = this;
  this._connection = new Connection(options);
  this.apiCallCount = this._connection.apiCallCount;
  this._connection.on('apiCall', function(data) {
    self.emit('apiCall', data);
  });

  this.campaign = new Campaign(this, this._connection);
  this.email = new Email(this, this._connection);
  this.landingPage = new LandingPage(this, this._connection);
  this.list = new List(this, this._connection);
  this.lead = new Lead(this, this._connection);
  this.stats = new Stats(this, this._connection);
  this.activities = new Activities(this, this._connection);
  this.bulkLeadExtract = new BulkLeadExtract(this, this._connection);
  this.bulkActivityExtract = new BulkActivityExtract(this, this._connection);
}

util.inherits(Marketo, EventEmitter);

Marketo.prototype.getOAuthToken = function oauthToken() {
  return this._connection.getOAuthToken(true);
};

Marketo.prototype.streamify = function streamify(object, method, args, options) {
  return new MarketoStream(this, object, method, args, options);
};

module.exports = Marketo;
