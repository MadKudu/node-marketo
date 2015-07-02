var assert = require('assert'),
    _ = require('lodash'),
    marketo = require('./helper/connection');

describe('Lists', function() {
  describe('#find', function() {
    it('finds a list by id', function(done) {
      marketo.list.find({id: 1}).then(function(response) {
        assert.equal(response.result.length, 1);
        assert.equal(response.result[0].id, 1001);
        assert(_.has(response.result[0], 'name'));
        assert(_.has(response.result[0], 'workspaceName'));
        done();
      });
    });
    it('finds a list by name and workspaceName', function(done) {
      marketo.list.find({name: 'Lions', workspaceName: 'Default'}).then(function(response) {
        assert.equal(response.result.length, 1);
        assert.equal(response.result[0].id, 1001);
        assert(_.has(response.result[0], 'name'));
        assert.equal(response.result[0].name, 'Lions');
        assert(_.has(response.result[0], 'workspaceName'));
        assert.equal(response.result[0].workspaceName, 'Default');
        done();
      });
    });
  });
  describe('#byId', function() {
    it('finds a list by id', function(done) {
      marketo.list.byId({id: 1}).then(function(response) {
        assert.equal(response.result.length, 1);
        assert.equal(response.result[0].id, 1001);
        assert(_.has(response.result[0], 'name'));
        assert(_.has(response.result[0], 'workspaceName'));
        done();
      });
    });
  });
  describe('#isMember', function() {
    it('finds a lead as member of list', function(done) {
      marketo.list.isMember(1001, 42).then(function(response) {
        assert.equal(response.result.length, 1);
        assert.equal(response.result[0].id, 42);
        assert(_.has(response.result[0], 'status'));
        assert.equal(response.result[0].status, 'memberof');
        done();
      });
    });
    it('does not find a lead missing from a list', function(done) {
      marketo.list.isMember(1001, 44).then(function(response) {
        assert.equal(response.result.length, 1);
        assert.equal(response.result[0].id, 44);
        assert(_.has(response.result[0], 'status'));
        assert.equal(response.result[0].status, 'notmemberof');
        done();
      });
    });
  });
  describe('#removeLeadsFromList', function() {
    it('removes a lead from a list', function(done) {
      marketo.list.removeLeadsFromList(1001, 42).then(function(response) {
        assert.equal(response.result.length, 1);
        assert.equal(response.result[0].id, 42);
        assert(_.has(response.result[0], 'status'));
        assert.equal(response.result[0].status, 'removed');
        done();
      });
    });
  });
});
