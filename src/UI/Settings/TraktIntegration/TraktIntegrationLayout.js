var Marionette = require('marionette');
var AsModelBoundView = require('../../Mixins/AsModelBoundView');
var vent = require('vent');
var $ = require('jquery');
var AppLayout = require('../../AppLayout');
window.vent = vent;

var _this;
var view = Marionette.ItemView.extend({
    template : 'Settings/TraktIntegration/TraktIntegrationLayoutTemplate',

    events: {
        'click .x-authorize'   : '_authorize',
        'click .x-unauthorize' : '_unauthorize'
    },

    modelEvents: {
        'change': 'render'
    },

    ui: {
        authorizeBtn   : '.x-authorize',
        unauthorizeBtn : '.x-unauthorize'
    },

    initialize: function() {
        this.listenTo(this.model, 'sync', this.render);
        this.listenTo(vent, vent.Events.TraktCredsChanged, this._refresh);
        this.model.fetch();
        _this = this;
        window.trakt = this;
    },

    _unauthorize: function() {
        var promise = $.ajax({url: window.NzbDrone.ApiRoot + "/traktcredentials", method: "DELETE"});
        promise.then(function (res) {
            console.log("Unauthorized Trakt credentials successfully.");
            _this.model.fetch();
        }, function(err) {
            console.error("Failed to unauthorize Trakt credentials: " + err);
        });
    },

    _authorize: function() {
        var promise = $.ajax({url: window.NzbDrone.ApiRoot + "/traktcredentials/oauth/redirecturl" +
                                        "?clientId=" + this.model.get('clientId') +
                                        "&secret=" + this.model.get('clientSecret') +
                                        "&apikey=" + window.NzbDrone.ApiKey +
                                        "&origin=" + document.URL,
                             async: false
                             });
        promise.then(function (url) {
            window.open(url, 'authWindow');

        });
    },

    _refresh: function(obj) {
        console.log('refresh called: ' + JSON.stringify(obj));
        _this.model.fetch();
        AppLayout.modalRegion.show(view);
    }
});

module.exports = AsModelBoundView.call(view);

