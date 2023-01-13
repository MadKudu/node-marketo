const _ = require('lodash'),
  Promise = require('bluebird'),
  assert = require('assert'),
  util = require('../lib/util'),
  nock = require('nock'),
  rewire = require('rewire');

const Connection = rewire('../lib/connection.js');
Connection.__set__('getNextPageFn', getNextPageFn);
function getNextPageFn(conn, method, args) {
  var assetStrategy = {
    offset: () => {
      return () => 'offset';
    },
    token: () => {
      return () => 'token';
    },
  };

  var options = _.clone(_.last(args) || {});
  args = _.clone(args);
  args.pop();

  var selectedStrategy = assetStrategy[util.nextPageType(args[0])];
  return selectedStrategy(conn, method, args, options);
}

const assetScope = nock('http://localhost-mock')
  .filteringRequestBody((body) => {
    console.log(body);
    return true;
  })
  .post('/asset')
  .reply(200, {
    success: true,
    errors: [],
    requestId: '6efc#16c8967a21f',
    warnings: [],
    result: [
      {
        id: 4363,
        name: 'Smart List Test 01',
      },
    ],
  });

const tokenScope = nock('http://localhost-mock')
  .filteringRequestBody((body) => {
    console.log(body);
    return true;
  })
  .post('/rest/v1')
  .reply(200, {
    moreResult: true,
    nextPageToken: 'string',
    requestId: 'string',
    result: [
      {
        id: 0,
        status: 'string',
      },
    ],
    success: true,
  });

const ASSET_URL = 'asset',
  TOKEN_URL = 'rest/v1',
  IDENTITY_URL = 'identity';

Connection.prototype.getOAuthToken = function () {
  return Promise.resolve({ access_token: 'test_token' });
};

function getUrl(path) {
  return 'http://localhost-mock/' + path;
}

function getConnection() {
  var options = {
    endpoint: getUrl(''),
    identity: getUrl(IDENTITY_URL),
    clientId: 'someId',
    clientSecret: 'someSecret',
  };
  return new Connection(options);
}

describe('Connection', function () {
  it('token type pagination', function () {
    return getConnection()
      .post(TOKEN_URL, { data: { _method: 'GET', maxReturn: 200 } })
      .then((resp) => {
        assert.equal(resp.nextPage(), 'token');
      });
  });

  it('offset type pagination', function () {
    return getConnection()
      .post(ASSET_URL, { data: { _method: 'GET', maxReturn: 200 } })
      .then((resp) => {
        assert.equal(resp.nextPage(), 'offset');
      });
  });
});
