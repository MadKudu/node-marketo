var _ = require('lodash'),
    Connection = require('./connection'),
    Lead = require('./api/lead'),
    List = require('./api/list');

function Marketo(options) {
  this._connection = new Connection(options);

  this.list = new List(this, this._connection);
  this.lead = new Lead(this, this._connection);
}

module.exports = Marketo;
