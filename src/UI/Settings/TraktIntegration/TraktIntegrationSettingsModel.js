var SettingsModelBase = require('../SettingsModelBase');


module.exports = SettingsModelBase.extend({
    url            : window.NzbDrone.ApiRoot + '/traktcredentials',
    successMessage : 'Trakt credentials saved',
    errorMessage   : 'Failed to save Trakt credentials'
});