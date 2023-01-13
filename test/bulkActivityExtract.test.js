var assert = require('assert'),
  _ = require('lodash'),
  moment = require('moment'),
  marketo = require('./helper/connection');

let hasReplay = false;
let createRequest = () => marketo.bulkActivityExtract.create({ createdAt: { startAt: moment.utc('2023-01-07'), endAt: moment.utc('2023-01-08') } });
let cancelRequest = (res, done) => marketo.bulkActivityExtract.cancel(res.result[0].exportId).then(() => done()).catch(done);

describe('Bulk Activity Extract', function () {
  describe('#create', function () {
    it('creates an extract', function (done) {
      createRequest().then(function (response) {
        assert(_.has(response, 'requestId'));
        assert.equal(response.result.length, 1);
        assert.equal(response.result[0].status, 'Created');

        // cancel job
        cancelRequest(response, done);
      }).catch(done);
    });
  });

  describe('#enqueue', function () {
    it('enqueus an extract', function (done) {
      createRequest().then(function (res) {
        let exportId = res.result[0].exportId;
        marketo.bulkActivityExtract.enqueue(exportId).then(function (response) {
          assert.equal(response.result.length, 1);
          assert.equal(response.result[0].status, 'Queued');

          // cancel job
          cancelRequest(response, done);
        }).catch(done);
      }).catch(done);
    });
  });

  describe('#status', function () {
    it('checks the status of an extract', function (done) {
      createRequest().then(function (res) {
        let exportId = res.result[0].exportId;
        marketo.bulkActivityExtract.status(exportId).then(function (response) {
          assert.equal(response.result.length, 1);
          assert(_.has(response.result[0], 'status'));

          // cancel job
          cancelRequest(response, done);
        }).catch(done);
      }).catch(done);
    });
  });

  if (hasReplay)
    describe('#statusTillCompleted', function () {
      it('checks the status of an extract till complete', function (done) {
        this.timeout(60000 * 10);
        createRequest().then(function (res) {
          let exportId = res.result[0].exportId;
          marketo.bulkActivityExtract.enqueue(exportId).then(function (res) {
            marketo.bulkActivityExtract.statusTillCompleted(exportId).then(function (response) {
              assert.equal(response.result.length, 1);
              assert.equal(response.result[0].status, 'Completed');
              done();
            }).catch(done);
          }).catch(done);
        }).catch(done);
      });
    });

  describe('#cancel', function () {
    it('cancels an extract', function (done) {
      createRequest().then(function (res) {
        let exportId = res.result[0].exportId;
        marketo.bulkActivityExtract.cancel(exportId).then(function (response) {
          assert.equal(response.result.length, 1);
          assert.equal(response.result[0].status, 'Cancelled');
          done();
        }).catch(done);
      }).catch(done);
    });
  });

  if (hasReplay)
    describe.only('#get', function () {
      it('coordinates an extract until completed', function (done) {
        this.timeout(60000 * 10);
        marketo.bulkActivityExtract.get({ createdAt: { startAt: moment(), endAt: moment() } }).then(function (response) {
          assert.equal(response.result.length, 1);
          assert.equal(response.result[0].status, 'Completed');
          done();
        }).catch(done);
      });
    });

  describe('#file', function () {
    it('downloads an extract file', function (done) {
      done();
      // not tested - need file handle
      // marketo.bulkActivityExtract.file(1).then(function (response) {
      //   assert.equal(response.result.length, 1);
      //   done();
      // }).catch(done);
    });
  });

  describe('#fileStream', function () {
    it('streams an extract file', function (done) {
      done();
      // not tested - need file handle
      // marketo.bulkActivityExtract.fileStream(1).then(function (response) {
      //   assert.equal(response.result.length, 1);
      //   done();
      // }).catch(done);
    });
  });
});
