var assert = require('assert'),
  _ = require('lodash'),
  marketo = require('./helper/connection');

describe('SmartLists', function () {
  describe('#find', function () {
    it('displays list of smart lists', function (done) {
      marketo.smartList
        .find({ maxReturn: 100 })
        .then(function (response) {
          assert.equal(response.success, true);
          assert(_.has(response.result[0], 'name'));
          assert(_.has(response.result[0], 'workspace'));
          smartListId = response.result[0].id;
          done();
        })
        .catch(done);
    });
  });
  describe('#byId', function () {
    it('finds smart list by id', function (done) {
      marketo.smartList
        .byId(1, { includeRules: true })
        .then(function (response) {
          assert.equal(response.success, true);
          assert.equal(response.result.length, 1);
          assert(_.has(response.result[0], 'name'));
          assert(_.has(response.result[0], 'workspace'));
          done();
        })
        .catch(done);
    });
  });
});
