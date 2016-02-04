var _ = require('lodash'),
    Connection = require('./connection'),
    Lead = require('./api/lead'),
    List = require('./api/list'),
    Stats = require('./api/stats'),
    Campaign = require('./api/campaign'),
    MarketoStream = require('./stream');

function Marketo(options) {
  this._connection = new Connection(options);

  this.list = new List(this, this._connection);
  this.lead = new Lead(this, this._connection);
  this.stats = new Stats(this, this._connection);
  this.campaign = new Campaign(this, this._connection);
}

Marketo.prototype = {
  getOAuthToken: function oauthToken() {
    return this._connection.getOAuthToken(true);
  }
};

Marketo.streamify = function streamify(marketoResult) {
  return new MarketoStream(marketoResult);
};

module.exports = Marketo;
