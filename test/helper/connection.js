var _ = require('lodash'),
    Replay = require('replay'),
    config = require('./config'),
    Marketo = require('../../index');

// Remove authorization from the header comparison
Replay.headers = _.filter(Replay.headers, function(header) {
  return !(header.toString() === '/^authorization/');
});

module.exports = new Marketo(config.creds.computed);
