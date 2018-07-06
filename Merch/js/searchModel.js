// var aa=(function () {
// 构造
var seachModel = function (options) {

    let container1 = document.getElementById(options.elem1); //容器
    let container2 = document.getElementById(options.elem2); //容器
    let pagename = options.pagename;  //数据
    let processname = options.processname;  //数据
    let searchBtn=document.getElementById(options.searchBtn);

    let token = options.token;//选中的子节点  [{ID:GameID}]
    let form = options.form; //layui from对象
    let layer = options.layer; //layui from对象
    let d = [];
    let _domStr1 = "";  //结构字符串
    let _domStr2 = "";  //结构字符串

    let templateDetails=[];//保存模板条件
    let conditions=[];//查询条件
    let obj = {
        'pagename': pagename,
        'processname': processname,
        'userToken': token, 'signkey': '1f626576304bf5d95b72ece2222e42c3'
    }
    let _parseJson = JSON.stringify(obj);
    $.ajax({
        type: 'post',
        url: '/Query?action=init',
        contentType: "application/json; charset=utf-8",
        data: {parasJson: _parseJson},
        async: false,
        success: function (data) {
            data = JSON.parse(data);
            if (data.result_code == 1) {
                d = data.result_data;
                console.log(d);
                if (d.length > 0) {

                    for (i in d) {
                        if (d[i].issearch == 1) {
                            _domStr1 += '<input type="checkbox" checked lay-skin="primary" value="' + d[i].id + '" title="' + d[i].title + '" lay-filter="checkList"/><br>'
                        } else {
                            _domStr1 += '<input type="checkbox" lay-skin="primary" value="' + d[i].id + '" title="' + d[i].title + '" lay-filter="checkList"/><br>'
                        }

                            if (d[i].type == 'literals') {
                                _domStr2 += '<div class="layui-inline ' + d[i].field + '">' +
                                    '<label class="layui-form-label">' + d[i].title + '</label>';
                                _domStr2 += '<div class="layui-input-inline"><select id="' + d[i].field + '" lay-filter="' + d[i].field + '">';
                                let html = '<option value="">-请选择-</option>';
                                for (j in d[i].list) {
                                    html += '<option value="' + j + '">' + d[i].list[j] + '</option>'
                                }
                                __domStr2 += html;
                                _domStr2 += '</select></div></div>';
                            } else if (d[i].type == 'bit') {
                                _domStr2 += ' <div class="layui-inline ' + d[i].field + '"><label class="layui-form-label">' + d[i].title + '</label>' +
                                    '<div class="layui-input-inline" style="width: 188px;height: 36px;border: 1px solid #eee;background-color: #fff;">' +
                                    '<input type="radio" name="' + d[i].field + '" value="1" title="是" checked="" lay-filter="' + d[i].field + '">' +
                                    '<input type="radio" name="' + d[i].field + '" value="0" title="否" lay-filter="' + d[i].field + '">' +
                                    '</div></div>'
                            } else if (d[i].type == 'string') {
                                _domStr2 += ' <div class="layui-inline ' + d[i].field +'">' +
                                    '<label class="layui-form-label">' + d[i].title + '</label>' +
                                    '<div class="layui-input-inline">' +
                                    '<input type="text" id="' + d[i].field + '" class="layui-input">' +
                                    '</div></div>'
                            } else if (d[i].type == 'number') {
                                if (d[i].condition == 5) {
                                    _domStr2 += ' <div class="layui-inline ' + d[i].field +'"><label class="layui-form-label">' + d[i].title + '</label>' +
                                        '<div class="layui-input-inline"><select>' + setOption(d[i].condition) + '</select></div>' +
                                        '<div class="layui-input-inline" style="width: 100px">' +
                                        '<input type="text" id="' + d[i].field + '1" class="layui-input">' +
                                        '</div>' +
                                        '<div class="layui-form-mid">至</div>' +
                                        '<div class="layui-input-inline" style="width: 100px">' +
                                        '<input type="text" id="' + d[i].field + '2" class="layui-input">' +
                                        '</div>' +
                                        '</div>'
                                } else {
                                    _domStr2 += ' <div class="layui-inline ' + d[i].field + ' "><label class="layui-form-label">' + d[i].title + '</label>' +
                                        '<div class="layui-input-inline"><select>' + setOption(d[i].condition) + '</select></div>' +
                                        '<div class="layui-input-inline">' +
                                        '<input type="text" id="' + d[i].field + '1" class="layui-input">' +
                                        '</div></div>'
                                }

                            } else if (d[i].type == 'time') {
                                if (d[i].condition == 5) {
                                    _domStr2 += ' <div class="layui-inline ' + d[i].field + '"><label class="layui-form-label">' + d[i].title + '</label>' +
                                        '<div class="layui-input-inline"><select>' + setOption(d[i].condition) + '</select></div>' +
                                        '<div class="layui-input-inline" style="width: 100px">' +
                                        '<input type="text" id="' + d[i].field + '1" class="layui-input">' +
                                        '</div>' +
                                        '<div class="layui-form-mid">至</div>' +
                                        '<div class="layui-input-inline" style="width: 100px">' +
                                        '<input type="text" id="' + d[i].field + '2" class="layui-input">' +
                                        '</div>' +
                                        '</div>'
                                } else {
                                    _domStr2 += ' <div class="layui-inline ' + d[i].field + '"><label class="layui-form-label">' + d[i].title + '</label>' +
                                        '<div class="layui-input-inline"><select>' + setOption(d[i].condition) + '</select></div>' +
                                        '<div class="layui-input-inline">' +
                                        '<input type="text" id="' + d[i].field + '1" class="layui-input">' +
                                        '</div></div>'
                                }
                            } else if (d[i].type == 'date') {
                                if (d[i].condition == 5) {
                                    _domStr2 += ' <div class="layui-inline" ' + d[i].field + '><label class="layui-form-label">' + d[i].title + '</label>' +
                                        '<div class="layui-input-inline"><select>' + setOption(d[i].condition) + '</select></div>' +
                                        '<div class="layui-input-inline" style="width: 100px">' +
                                        '<input type="text" id="' + d[i].field + '1" class="layui-input">' +
                                        '</div>' +
                                        '<div class="layui-form-mid">至</div>' +
                                        '<div class="layui-input-inline" style="width: 100px">' +
                                        '<input type="text" id="' + d[i].field + '2" class="layui-input">' +
                                        '</div>' +
                                        '</div>'
                                } else {
                                    _domStr2 += ' <div class="layui-inline" ' + d[i].field + '><label class="layui-form-label">' + d[i].title + '</label>' +
                                        '<div class="layui-input-inline"><select>' + setOption(d[i].condition) + '</select></div>' +
                                        '<div class="layui-input-inline">' +
                                        '<input type="text" id="' + d[i].field + '1" class="layui-input">' +
                                        '</div></div>'
                                }
                            }

                        }

                    container1.innerHTML = _domStr1;
                    container2.innerHTML = _domStr2;
                    for (i in d) {
                    if(d[i].issearch==0){
                        $('.'+d[i].field).addClass('layui-hide')
                    }

                    }
                    form.render();

                }

            } else {
                _this.layer.msg(data.result_msg || data.return_msg);
            }
        }
    });
    form.on('checkbox(checkList)', function (data) {
        let _val = data.value;
        if (data.elem.checked) {
            for (i in d) {
                if (d[i].id == _val) {
                    d[i].issearch = 1;
                    container2.getElementsByClassName(d[i].field)[0].classList.remove('layui-hide')
                }
            }
        } else {
            for (i in d) {
                if (d[i].id == _val) {
                    d[i].issearch = 0;
                    container2.getElementsByClassName(d[i].field)[0].classList.add('layui-hide')
                }

            }
        }
    });
    searchBtn.addEventListener('click',function () {
        for(i in d){
            if(d[i].issearch==1){
                if(d[i].type=='string'){
                    conditions.push({'id':'','field':d[i].field,'values':$('#'+d[i].field).val()})
                }
            }
        }
        console.log(conditions)
    })
}

function setOption(j) {
    let options = [{'value': '=', 'key': '0'},
        {'value': '>', 'key': '1'},
        {'value': '<', 'key': '2'},
        {'value': '>=', 'key': '3'},
        {'value': '<=', 'key': '4'},
        {'value': '介于', 'key': '5'},
        {'value': '包含', 'key': '6'}]
    let html = '';
    for (let i in options) {
        if (options[i].key == j) {
            html += '<option value="' + options[i].key + ' selected">' + options[i].value + '</option>'
        } else {
            html += '<option value="' + options[i].key + '">' + options[i].value + '</option>'
        }
    }
    return html;
}

// window.seachModel=seachModel;window
// })()


let token1 = window.localStorage.getItem('token');
layui.use(['form', 'layer', 'table'], () => {
    const form = layui.form;
    const layer = layui.layer;
    const table = layui.table;

    seachModel({
        'elem1': 'tantion',
        'elem2': 'serchMode',
        'pagename': 'projectInfoSearch',
        'processname': 'projectInfoSearch',
        'token': token1,
        'form': form,
        'layer': layer,
        'searchBtn':'searchBtn'
    })
    $('#form2Box').click(function () {
        $('#form2').slideDown();
    })
})