var dasJs = dasJs || {};

dasJs.enableFormControlsOnChange = {
  init: function (radioGroupsRequiredToEnable) {
    if (!document.querySelectorAll('input[data-disabled]').length) return false;
    var disabledInputs = document.querySelectorAll('input[data-disabled]');
    this.handleDisabledInputs(radioGroupsRequiredToEnable, disabledInputs)
  },

  handleDisabledInputs: function (radioGroupsRequiredToEnable, disabledInputs) {
    function allRequiredRadiosChecked() {
      for (var i = 0; i < radioGroupsRequiredToEnable.length; i++) {
        if (!document.querySelector('input[name="' + radioGroupsRequiredToEnable[i] + '"]:checked')) return false;
      }
      return true;
    }

    if (!allRequiredRadiosChecked()) {
      for (var i = 0; i < disabledInputs.length; i++) {
        disabledInputs[i].disabled = true
      }
      this.handleFormControls(radioGroupsRequiredToEnable, disabledInputs);
    }
  },

  handleFormControls: function (radioGroupsRequiredToEnable, disabledInputs) {
    document.addEventListener("click", function (event) {
      if (event.target.name === undefined) return false;

      for (var i = 0; i < radioGroupsRequiredToEnable.length; i++) {
        if (event.target.name === radioGroupsRequiredToEnable[i]) {
          radioGroupsRequiredToEnable.splice(radioGroupsRequiredToEnable.indexOf(event.target.name), 1);

          if (radioGroupsRequiredToEnable.length < 1 || radioGroupsRequiredToEnable === undefined) {
            for (var i = 0; i < disabledInputs.length; i++) {
              disabledInputs[i].disabled = false
            }
          }
        }
      }
    })
  }
};