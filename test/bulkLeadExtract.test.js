var assert = require('assert'),
  _ = require('lodash'),
  marketo = require('./helper/connection');

describe('Bulk Lead Extract', function () {
  describe('#create', function () {
    it('creates an extract', function (done) {
      marketo.bulkLeadExtract.create([], {}).then(function (response) {
        assert.equal(response.result.length, 1);
        assert.equal(response.result[0].id, 1001);
        assert(_.has(response.result[0], 'name'));
        assert(_.has(response.result[0], 'workspaceName'));
        done();
      });
    });
  });
  describe('#enqueue', function () {
    it('enqueus an extract', function (done) {
      marketo.bulkLeadExtract.enqueue(1).then(function (response) {
        assert.equal(response.result.length, 1);
        assert.equal(response.result[0].id, 1001);
        assert(_.has(response.result[0], 'name'));
        assert(_.has(response.result[0], 'workspaceName'));
        done();
      });
    });
  });
  describe('#status', function () {
    it('checks the status of an extract', function (done) {
      marketo.bulkLeadExtract.status(1).then(function (response) {
        assert.equal(response.result.length, 1);
        assert.equal(response.result[0].id, 42);
        assert(_.has(response.result[0], 'status'));
        assert.equal(response.result[0].status, 'memberof');
        done();
      });
    });
  });
  describe('#statusTilComplete', function () {
    it('checks the status of an extract till complete', function (done) {
      marketo.bulkLeadExtract.statusTilComplete(1).then(function (response) {
        assert.equal(response.result.length, 1);
        assert.equal(response.result[0].id, 42);
        assert(_.has(response.result[0], 'status'));
        assert.equal(response.result[0].status, 'memberof');
        done();
      });
    });
  });
  describe('#cancel', function () {
    it('cancels an extract', function (done) {
      marketo.bulkLeadExtract.cancel(1).then(function (response) {
        assert.equal(response.result.length, 1);
        assert.equal(response.result[0].id, 42);
        assert(_.has(response.result[0], 'status'));
        assert.equal(response.result[0].status, 'removed');
        done();
      });
    });
  });
  describe('#get', function () {
    it('coordinates an extract until completed', function (done) {
      marketo.bulkLeadExtract.get([], {}).then(function (response) {
        assert.equal(response.result.length, 1);
        assert.equal(response.result[0].id, 42);
        assert(_.has(response.result[0], 'status'));
        assert.equal(response.result[0].status, 'removed');
        done();
      });
    });
  });
  describe('#file', function () {
    it('downloads an extract file', function (done) {
      marketo.bulkLeadExtract.file(1).then(function (response) {
        assert.equal(response.result.length, 1);
        assert.equal(response.result[0].id, 42);
        assert(_.has(response.result[0], 'status'));
        assert.equal(response.result[0].status, 'removed');
        done();
      });
    });
  });
  describe('#fileStream', function () {
    it('streams an extract file', function (done) {
      marketo.bulkLeadExtract.fileStream(1).then(function (response) {
        assert.equal(response.result.length, 1);
        assert.equal(response.result[0].id, 42);
        assert(_.has(response.result[0], 'status'));
        assert.equal(response.result[0].status, 'removed');
        done();
      });
    });
  });
});
