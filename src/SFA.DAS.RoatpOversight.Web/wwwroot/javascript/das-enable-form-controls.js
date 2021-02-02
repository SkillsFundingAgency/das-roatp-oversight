var dasJs = dasJs || {};

dasJs.enableFormControlsOnChange = {
  init: function (radioGroupsRequiredToEnable) {
    if (!document.querySelectorAll('input[data-module="das-disabled"]').length || !radioGroupsRequiredToEnable.length) return false;

    var disabledInputs = document.querySelectorAll('input[data-module="das-disabled"]');
    this.handleDisabledInputs(radioGroupsRequiredToEnable, disabledInputs)
  },

  handleDisabledInputs: function (radioGroupsRequiredToEnable, disabledInputs) {
    var radioGroupsStillRequired = [];

    function noRadiosInGroupChecked(groupName) {
      var radiosInGroupNodeList = document.querySelectorAll('input[name="' + groupName + '"]');
      for (var j = 0; j < radiosInGroupNodeList.length; j++) {
        if (radiosInGroupNodeList[j].checked) return false;
      }
      return true;
    }

    for (var i = 0; i < radioGroupsRequiredToEnable.length; i++) {
      if (noRadiosInGroupChecked(radioGroupsRequiredToEnable[i])) {
        radioGroupsStillRequired.push(radioGroupsRequiredToEnable[i])
      }
    }

    if (radioGroupsStillRequired.length) {
      for (var i = 0; i < disabledInputs.length; i++) {
        disabledInputs[i].disabled = true
      }

      this.handleFormControls(radioGroupsStillRequired, disabledInputs);
    }
  },

  handleFormControls: function (radioGroupsStillRequired, disabledInputs) {
    document.addEventListener("click", function (event) {
      if (event.target.name === undefined) return false;

      for (var i = 0; i < radioGroupsStillRequired.length; i++) {
        if (event.target.name === radioGroupsStillRequired[i]) {
          radioGroupsStillRequired.splice(radioGroupsStillRequired.indexOf(event.target.name), 1);

          if (radioGroupsStillRequired.length < 1 || radioGroupsStillRequired === undefined) {
            for (var i = 0; i < disabledInputs.length; i++) {
              disabledInputs[i].disabled = false
            }
          }
        }
      }
    })
  }
};