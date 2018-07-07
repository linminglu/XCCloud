// var aa=(function () {
// 构造
var seachModel = function (options) {

    let container1 = document.getElementById(options.elem1); //容器
    let container2 = document.getElementById(options.elem2); //容器
    let pagename = options.pagename;  //数据
    let processname = options.processname;  //数据
    let searchBtn = document.getElementById(options.searchBtn);

    let token = options.token;//选中的子节点  [{ID:GameID}]
    let form = options.form; //layui from对象
    let layer = options.layer; //layui from对象

    let xc=options.xc;
    let parm=options.parm;
    let d = [];
    let _domStr1 = "";  //结构字符串
    let _domStr2 = "";  //结构字符串

    let templateDetails = [];//保存模板条件
    let conditions = [];//查询条件
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
                            _domStr2 += '<div class="layui-inline ' + d[i].field + 'sh">' +
                                '<label class="layui-form-label">' + d[i].title + '</label>';
                            _domStr2 += '<div class="layui-input-inline"><select id="' + d[i].field + 'sh" lay-filter="' + d[i].field + 'sh">';
                            let html = '<option value="">-请选择-</option>';
                            for (j in d[i].list) {
                                html += '<option value="' + j + '">' + d[i].list[j] + '</option>'
                            }
                            __domStr2 += html;
                            _domStr2 += '</select></div></div>';
                        } else if (d[i].type == 'bit') {
                            _domStr2 += ' <div class="layui-inline ' + d[i].field + 'sh"><label class="layui-form-label">' + d[i].title + '</label>' +
                                '<div class="layui-input-inline" style="width: 188px;height: 36px;border: 1px solid #eee;background-color: #fff;">' +
                                '<input type="radio" name="' + d[i].field + 'sh" value="1" title="是" checked="" lay-filter="' + d[i].field + 'sh">' +
                                '<input type="radio" name="' + d[i].field + 'sh" value="0" title="否" lay-filter="' + d[i].field + 'sh">' +
                                '</div></div>'
                        } else if (d[i].type == 'string') {
                            _domStr2 += ' <div class="layui-inline ' + d[i].field + 'sh">' +
                                '<label class="layui-form-label">' + d[i].title + '</label>' +
                                '<div class="layui-input-inline">' +
                                '<input type="text" id="' + d[i].field + 'sh" class="layui-input">' +
                                '</div></div>'
                        } else if (d[i].type == 'number') {
                            _domStr2 += ' <div class="layui-inline ' + d[i].field + 'sh">' +
                                            '<label class="layui-form-label">' + d[i].title + '</label>' +
                                            '<div class="layui-input-inline" style="width: 75px;margin-right: 0">' +
                                                 '<select id="' + d[i].field + 'cond">' + setOption(d[i].condition) + '</select>' +
                                            '</div>'+
                                            '<div class="layui-input-inline layui-hide" style="width: 100px">' +
                                                '<input type="text" id="' + d[i].field + 'sh1" class="layui-input">' +
                                            '</div>' +
                                            '<div class="layui-form-mid  ' + d[i].field + 'mid">至</div>' +
                                            '<div class="layui-input-inline layui-hide" style="width: 100px">' +
                                                '<input type="text" id="' + d[i].field + 'sh2" class="layui-input">' +
                                            '</div>'+
                                            '<div class="layui-input-inline layui-hide" style="width: 115px">' +
                                                 '<input type="text" id="' + d[i].field + 'sh11" class="layui-input">' +
                                            '</div>' +
                                        '</div>'

                        } else if (d[i].type == 'time') {
                            _domStr2 += ' <div class="layui-inline ' + d[i].field + 'sh">' +
                                '<label class="layui-form-label">' + d[i].title + '</label>' +
                                '<div class="layui-input-inline" style="width: 75px;margin-right: 0">' +
                                '<select id="' + d[i].field + 'cond">' + setOption(d[i].condition) + '</select>' +
                                '</div>'+
                                '<div class="layui-input-inline layui-hide" style="width: 100px">' +
                                '<input type="text" id="' + d[i].field + 'sh1" class="layui-input">' +
                                '</div>' +
                                '<div class="layui-form-mid  ' + d[i].field + 'mid">至</div>' +
                                '<div class="layui-input-inline layui-hide" style="width: 100px">' +
                                '<input type="text" id="' + d[i].field + 'sh2" class="layui-input">' +
                                '</div>'+
                                '<div class="layui-input-inline layui-hide" style="width: 115px">' +
                                '<input type="text" id="' + d[i].field + 'sh11" class="layui-input">' +
                                '</div>' +
                                '</div>'
                        } else if (d[i].type == 'date') {
                            _domStr2 += ' <div class="layui-inline ' + d[i].field + 'sh">' +
                                '<label class="layui-form-label">' + d[i].title + '</label>' +
                                '<div class="layui-input-inline" style="width: 75px;margin-right: 0">' +
                                '<select id="' + d[i].field + 'cond">' + setOption(d[i].condition) + '</select>' +
                                '</div>'+
                                '<div class="layui-input-inline layui-hide" style="width: 100px">' +
                                '<input type="text" id="' + d[i].field + 'sh1" class="layui-input">' +
                                '</div>' +
                                '<div class="layui-form-mid  ' + d[i].field + 'mid">至</div>' +
                                '<div class="layui-input-inline layui-hide" style="width: 100px">' +
                                '<input type="text" id="' + d[i].field + 'sh2" class="layui-input">' +
                                '</div>'+
                                '<div class="layui-input-inline layui-hide" style="width: 115px">' +
                                '<input type="text" id="' + d[i].field + 'sh11" class="layui-input">' +
                                '</div>' +
                                '</div>'

                        }

                    }

                    container1.innerHTML = _domStr1;
                    container2.innerHTML = _domStr2;
                    for (i in d) {
                        if (d[i].issearch == 0) {
                            $('.' + d[i].field + 'sh').addClass('layui-hide')
                        } else {
                            if (d[i].type == 'string') {
                                conditions.push({'id': d[i].tempId,'condition':'6', 'field': d[i].field, 'values': ''});
                            } else if (d[i].type == 'bit') {
                                conditions.push({'id': d[i].tempId, 'condition':'6','field': d[i].field, 'values': '1'});
                            } else if (d[i].type == 'literals') {
                                conditions.push({'id': d[i].tempId,'condition':'6', 'field': d[i].field, 'values': ''});
                            } else {
                                conditions.push({'id': d[i].tempId,'condition':d[i].condition, 'field': d[i].field, 'values': []});
                            }

                        }
                        if(d[i].type == 'number' || d[i].type == 'date' || d[i].type == 'time') {
                            if(d[i].condition==5){
                                $('#'+d[i].field+'sh1').parent('div').removeClass('layui-hide')
                                $('.'+d[i].field+'mid').removeClass('layui-hide')
                                $('#'+d[i].field+'sh2').parent('div').removeClass('layui-hide')
                                $('#'+d[i].field+'sh11').parent('div').addClass('layui-hide')
                            }else {
                                $('#'+d[i].field+'sh1').parent('div').addClass('layui-hide')
                                $('.'+d[i].field+'mid').addClass('layui-hide')
                                $('#'+d[i].field+'sh2').parent('div').addClass('layui-hide')
                                $('#'+d[i].field+'sh11').parent('div').removeClass('layui-hide')
                            }
                        }

                        //模板条件


                            if(d[i].userId==0){
                                templateDetails.push({'id':0,
                                    'tempId':0,
                                    'pagename':pagename,
                                    'processname':processname,
                                    'fieldName':d[i].field,
                                    'title':d[i].title,
                                    'dataType':d[i].type,
                                    'condition':d[i].condition,
                                    'width':d[i].width,
                                    'showColume':d[i].iscolume,
                                    'showSearch':d[i].issearch,
                                    'dictId':d[i].dictId})
                            }else {
                                templateDetails.push({'id':d[i].id,
                                    'tempId':d[i].tempId,
                                    'pagename':pagename,
                                    'processname':processname,
                                    'fieldName':d[i].field,
                                    'title':d[i].title,
                                    'dataType':d[i].type,
                                    'condition':d[i].condition,
                                    'width':d[i].width,
                                    'showColume':d[i].iscolume,
                                    'showSearch':d[i].issearch,
                                    'dictId':d[i].dictId})
                            }


                    }
                    form.render();

                }

            } else {
                _this.layer.msg(data.result_msg || data.return_msg);
            }
        }
    });
    for(i in d){
        if(d[i].type == 'date' ) {
            options.laydate.render({
                elem: '#'+d[i].field+'sh1'
                ,type: 'date'
            })
            options.laydate.render({
                elem: '#'+d[i].field+'sh2'
                ,type: 'date'
            })
            options.laydate.render({
                elem: '#'+d[i].field+'sh11'
                ,type: 'date'
            })


        }
        if( d[i].type == 'time') {
            options.laydate.render({
                elem: '#'+d[i].field+'sh1'
                ,type: 'time'
            })
            options.laydate.render({
                elem: '#'+d[i].field+'sh2'
                ,type: 'time'
            })
            options.laydate.render({
                elem: '#'+d[i].field+'sh11'
                ,type: 'time'
            })
        }
    }
    form.on('checkbox(checkList)', function (data) {
        console.log(data.elem)
        let _val = data.value;
        if (data.elem.checked) {
            for (i in d) {
                if (d[i].id == _val) {
                    d[i].issearch = 1;
                    container2.getElementsByClassName(d[i].field + 'sh')[0].classList.remove('layui-hide');
                    if (d[i].type == 'string') {
                        conditions.push({'id': d[i].tempId, 'condition':'6','field': d[i].field, 'values': ''});
                    } else if (d[i].type == 'bit') {
                        conditions.push({'id': d[i].tempId,'condition':'6', 'field': d[i].field, 'values': '1'});
                    } else if (d[i].type == 'literals') {
                        conditions.push({'id': d[i].tempId, 'condition':'6','field': d[i].field, 'values': ''});
                    } else {
                        conditions.push({'id': d[i].tempId,'condition':d[i].condition, 'field': d[i].field, 'values': []});
                    }
                }
            }

            for(i in templateDetails){
                if(templateDetails[i].title==data.elem.title){
                    templateDetails[i].showSearch=1
                }
            }

        } else {
            let index;
            for (i in d) {
                if (d[i].id == _val) {
                    index=i;
                    d[i].issearch = 0;
                    container2.getElementsByClassName(d[i].field + 'sh')[0].classList.add('layui-hide')
                }
            }
            for(i in conditions){
                if(conditions[i].field==d[index].field){
                    conditions.splice(i,1);
                }
            }
            for(i in templateDetails){
                if(templateDetails[i].title==d[index].title){
                    templateDetails[i].showSearch=0
                }
            }
// console.log(templateDetails)
        }
        event.stopPropagation();
    });

    form.on("radio()", function (data) {
        // console.log(data.elem)
        for (j in conditions) {
            if (conditions[j].field + 'sh' == data.elem.name) {
                conditions[j].values = data.value;
            }
        }
        // console.log(conditions)
    });
    form.on("select()", function (data) {
        // console.log(data.elem);
        for (j in d) {
            if (d[j].field + 'cond' == data.elem.id) {

                d[j].condition = data.value;
                if (data.value == 5) {
                    $('#'+d[j].field+'sh1').parent('div').removeClass('layui-hide')
                    $('.'+d[j].field+'mid').removeClass('layui-hide')
                    $('#'+d[j].field+'sh2').parent('div').removeClass('layui-hide')
                    $('#'+d[j].field+'sh11').parent('div').addClass('layui-hide')
                }else {
                    $('#'+d[j].field+'sh1').parent('div').addClass('layui-hide')
                    $('.'+d[j].field+'mid').addClass('layui-hide')
                    $('#'+d[j].field+'sh2').parent('div').addClass('layui-hide')
                    $('#'+d[j].field+'sh11').parent('div').removeClass('layui-hide')
                }
            }
        }
        for(i in conditions){
            if(conditions[i].field+'cond'==data.elem.id){
                conditions[i].condition=data.value
            }
        }
        for(i in templateDetails){
            if(templateDetails[i].fieldName+'cond'==data.elem.id){
                templateDetails[i].condition=data.value
            }
        }
    });

    searchBtn.addEventListener('click', function () {
        for (i in d) {
            if (d[i].issearch == 1) {
                for (j in conditions) {
                    if (conditions[j].field == d[i].field) {

                        if (d[i].type == 'string') {
                            conditions[j].values = $('#' + d[i].field + 'sh').val();
                        } else if (d[i].type == 'number' || d[i].type == 'date' || d[i].type == 'time') {
                            if ($('#' + d[i].field + 'cond').val() == 5) {
                                conditions[j].values = [$('#' + d[i].field + 'sh1').val(), $('#' + d[i].field + 'sh2').val()];
                            }else {
                                conditions[j].values = $('#' + d[i].field + 'sh11').val();
                            }
                        }
                    }
                }
            }
        }
        // console.log(conditions)
        parm.obj.conditions=conditions;
        // console.log(parm)
        xc.getInitData(parm)
        $('#'+options.elem1).parent().slideUp();

        console.log(templateDetails)
        let obj = {
            // 'pagename': pagename,
            'templateDetails': templateDetails,
            'userToken': token, 'signkey': '1f626576304bf5d95b72ece2222e42c3'
        }
        let _parseJson = JSON.stringify(obj);
        // console.log(obj)
        $.ajax({
            type: 'post',
            url: '/Query?action=save',
            contentType: "application/json; charset=utf-8",
            data: {parasJson: _parseJson},
            async: false,
            success: function (data) {
                data = JSON.parse(data);
                // console.log(data)
                if (data.result_code == 1) {

                }else {
                    layer.msg(data.result_msg||data.return_msg)
                }
            }
        })
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
            html += '<option value="' + options[i].key + '" selected>' + options[i].value + '</option>'
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


    $('#form2Box').toggle(function () {
        event.stopPropagation();
        $('#form2').slideDown();
    },function () {
        $('#form2').slideUp();
        event.stopPropagation();
    })
})