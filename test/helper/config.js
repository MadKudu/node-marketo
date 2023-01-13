var _ = require('lodash'),
  env = process.env,
  defaults = {
    endpoint: 'https://123-ABC-456.mktorest.com/rest',
    identity: 'https://123-ABC-456.mktorest.com/identity',
    clientId: 'someId',
    clientSecret: 'someSecret',
  },
  credentials;

credentials = {
  endpoint: env.MARKETO_ENDPOINT || defaults.endpoint,
  identity: env.MARKETO_IDENTITY || defaults.identity,
  clientId: env.MARKETO_CLIENT_ID || defaults.clientId,
  clientSecret: env.MARKETO_CLIENT_SECRET || defaults.clientSecret,
};

module.exports = {
  creds: {
    defaults: defaults,
    computed: credentials,
  },
};
