//验证字符串是否是数字
function checkNumber(theObj) {
    var reg = /^[0-9]+.?[0-9]*$/;
    if (reg.test(theObj)) {
        return true;
    }else {
        return false;
    }

}
