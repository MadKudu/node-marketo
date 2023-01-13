var assert = require('assert');
_ = require('lodash');
var config = require('./helper/config');
var Marketo = require('../lib/marketo');
var marketo = new Marketo({
  endpoint: config.creds.defaults.endpoint,
  identity: config.creds.defaults.identity,
  clientId: config.creds.defaults.clientId,
  clientSecret: config.creds.defaults.clientSecret
});

describe('Leads', function () {
  describe('#activities', function () {
    it('finds lead activities', function (done) {
      this.timeout(50000);
      marketo.lead.getPageToken('2016-07-15T13:22-07:00')
        .then(function (response) {
          var resultStream = Marketo.streamify(marketo.lead.getActivities([11696898], [3, 11, 13], response.nextPageToken));
          var count = 0;
          resultStream
            .on('data', function (data) {
              if (++count > 4) {
                // Closing stream, this CAN be called multiple times because the
                // buffer of the queue may already contain additional data
                resultStream.endMarketoStream();
              }
              console.log('DATA!!!', data);
            })
            .on('error', function (err) {
              // log the list error. Note, the stream closes if it encounters an error
              console.log('ERROR!!!', err);
            })
            .on('end', function () {
              // end of the stream
              // count here CAN be more than 20
              console.log('done, count is', count);
              done();
            });
        }).catch(done);
    });
  });

  describe('#byId', function () {
    it('finds a lead by id only', function (done) {
      marketo.lead.byId(1).then(function (response) {
        assert.equal(response.result.length, 1);
        assert.equal(response.result[0].id, 1);
        assert(_.has(response.result[0], 'email'));
        assert(_.has(response.result[0], 'lastName'));
        done();
      }).catch(done);
    });

    it('finds a lead but only retrieve the email field', function (done) {
      marketo.lead.byId(1, { fields: ['email'] }).then(function (resp) {
        assert.equal(resp.result.length, 1);

        var lead = resp.result[0];
        assert.equal(lead.id, 1);
        assert.equal(_.keys(lead).length, 2);
        assert(_.has(lead, 'email'));
        assert(!_.has(lead, 'lastName'));
        done();
      }).catch(done);
    });

    it('finds a lead and retrieve a subset of fields using csv', function (done) {
      marketo.lead.byId(1, { fields: 'email,lastName' }).then(function (resp) {
        assert.equal(resp.result.length, 1);

        var lead = resp.result[0];
        assert.equal(lead.id, 1);
        assert.equal(_.keys(lead).length, 3);
        assert(_.has(lead, 'email'));
        assert(_.has(lead, 'lastName'));
        done();
      }).catch(done);
    });
  });

  describe('#find', function () {
    it('uses a single filter value', function (done) {
      marketo.lead.find('id', [1]).then(function (resp) {
        assert.equal(resp.result.length, 1);

        var lead = resp.result[0];
        assert.equal(lead.id, 1);
        assert(_.has(lead, 'email'));
        assert(_.has(lead, 'lastName'));
        done();
      }).catch(done);
    });

    it('uses multiple filter values', function (done) {
      marketo.lead.find('id', [1, 2]).then(function (resp) {
        assert.equal(resp.result.length, 2);
        assert.equal(resp.result[0].id, 1);
        assert.equal(resp.result[1].id, 2);
        done();
      }).catch(done);
    });


    it('uses multiple filter values and retrieve a subset of fields', function (done) {
      marketo.lead.find('id', [1, 2], { fields: ['email', 'lastName'] }).then(function (resp) {
        assert.equal(resp.result.length, 2);
        assert.equal(resp.result[0].id, 1);
        assert.equal(resp.result[1].id, 2);

        var lead = resp.result[0];
        assert.equal(lead.id, 1);
        assert(_.has(lead, 'email'));
        assert(_.has(lead, 'lastName'));
        done();
      }).catch(done);
    });
  });

  describe('#createOrUpdate', function () {
    it('updates an existing record by using an email', function (done) {
      marketo.lead.createOrUpdate([{ email: 'ak+marketo1@usermind.com' }], { lookupField: 'email' })
        .then(function (resp) {
          assert.equal(resp.result.length, 1);
          assert.equal(resp.result[0].id, 1);
          assert.equal(resp.result[0].status, 'updated');
          done();
        }).catch(done);
    })
  });
});
