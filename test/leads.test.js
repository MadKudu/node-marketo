var assert = require('assert'),
    _ = require('lodash'),
    marketo = require('./helper/connection');

describe('Leads', function() {
  describe('#byId', function() {
    it('finds a lead by id only', function(done) {
      marketo.lead.byId(1).spread(function(response) {
        assert.equal(response.result.length, 1);
        assert.equal(response.result[0].id, 1);
        assert(_.has(response.result[0], 'email'));
        assert(_.has(response.result[0], 'lastName'));
        done();
      });
    });

    it('finds a lead but only retrieve the email field', function(done) {
      marketo.lead.byId(1, {fields: ['email']}).spread(function(resp) {
        assert.equal(resp.result.length, 1);

        var lead = resp.result[0];
        assert.equal(lead.id, 1);
        assert.equal(_.keys(lead).length, 2);
        assert(_.has(lead, 'email'));
        assert(!_.has(lead, 'lastName'));
        done();
      });
    });

    it('finds a lead and retrieve a subset of fields using csv', function(done) {
      marketo.lead.byId(1, {fields: 'email,lastName'}).spread(function(resp) {
        assert.equal(resp.result.length, 1);

        var lead = resp.result[0];
        assert.equal(lead.id, 1);
        assert.equal(_.keys(lead).length, 3);
        assert(_.has(lead, 'email'));
        assert(_.has(lead, 'lastName'));
        done();
      });
    });
  });

  describe('#find', function() {
    it('uses a single filter value', function(done) {
      marketo.lead.find('id', [1]).spread(function(resp) {
        assert.equal(resp.result.length, 1);

        var lead = resp.result[0];
        assert.equal(lead.id, 1);
        assert(_.has(lead, 'email'));
        assert(_.has(lead, 'lastName'));
        done();
      });
    });

    it('uses multiple filter values', function(done) {
      marketo.lead.find('id', [1,2]).spread(function(resp) {
        assert.equal(resp.result.length, 2);
        assert.equal(resp.result[0].id, 1);
        assert.equal(resp.result[1].id, 2);
        done();
      });
    });


    it('uses multiple filter values and retrieve a subset of fields', function(done) {
      marketo.lead.find('id', [1,2], {fields: ['email', 'lastName']}).spread(function(resp) {
        assert.equal(resp.result.length, 2);
        assert.equal(resp.result[0].id, 1);
        assert.equal(resp.result[1].id, 2);

        var lead = resp.result[0];
        assert.equal(lead.id, 1);
        assert(_.has(lead, 'email'));
        assert(_.has(lead, 'lastName'));
        done();
      });
    });
  });

  describe('#createOrUpdate', function() {
    it('updates an existing record by using an email', function(done) {
      marketo.lead.createOrUpdate([{email: 'ak+marketo1@usermind.com'}], {lookupField: 'email'})
        .spread(function(resp) {
          assert.equal(resp.result.length, 1);
          assert.equal(resp.result[0].id, 1);
          assert.equal(resp.result[0].status, 'updated');
          done();
        });
    })
  });
});
