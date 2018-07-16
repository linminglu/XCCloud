
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
    let types=options.active==null?'1':'0';
    let d = [];
    let _domStr1 = "";  //结构字符串
    let _domStr2 = "";  //结构字符串

    let templateDetails = [];//保存模板条件
    let conditions = [];//查询条件

    let _falg=false;//条件框的显示影藏
    let obj = {
        'pagename': pagename,
        'processname': processname,
        'userToken': token, 'signkey': '1f626576304bf5d95b72ece2222e42c3'
    };
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
                if (d.length > 0) {

                    for (let i in d) {
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
                            _domStr2 += html;
                            _domStr2 += '</select></div></div>';
                        } else if (d[i].type == 'bit') {
                            _domStr2 += ' <div class="layui-inline lay-bit ' + d[i].field + 'sh"><label class="layui-form-label">' + d[i].title + '</label>' +
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

                        }else if (d[i].type == 'datetime') {
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
                    for (let i in d) {
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
                layer.msg(data.result_msg || data.return_msg);
            }
        }
    });
    for(let i in d){
        if(d[i].type == 'date' ) {
            options.laydate.render({
                elem: '#'+d[i].field+'sh1'
                ,type: 'date'
            });
            options.laydate.render({
                elem: '#'+d[i].field+'sh2'
                ,type: 'date'
            });
            options.laydate.render({
                elem: '#'+d[i].field+'sh11'
                ,type: 'date'
            })


        }
        if( d[i].type == 'time') {
            options.laydate.render({
                elem: '#'+d[i].field+'sh1'
                ,type: 'time'
            });
            options.laydate.render({
                elem: '#'+d[i].field+'sh2'
                ,type: 'time'
            });
            options.laydate.render({
                elem: '#'+d[i].field+'sh11'
                ,type: 'time'
            })
        }
        if( d[i].type == 'datetime') {
            options.laydate.render({
                elem: '#'+d[i].field+'sh1'
                ,type: 'datetime'
            });
            options.laydate.render({
                elem: '#'+d[i].field+'sh2'
                ,type: 'datetime'
            });
            options.laydate.render({
                elem: '#'+d[i].field+'sh11'
                ,type: 'datetime'
            })
        }
    }
    form.on('checkbox(checkList)', function (data) {
        let _val = data.value;
        if (data.elem.checked) {
            for (let i in d) {
                if (d[i].id == _val) {
                    d[i].issearch = 1;
                    // console.log( container2.getElementsByClassName(d[i].field + 'sh')[0])
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

            for(let i in templateDetails){
                if(templateDetails[i].title==data.elem.title){
                    templateDetails[i].showSearch=1
                }
            }

        } else {
            let index;
            for (let i in d) {
                if (d[i].id == _val) {
                    index=i;
                    d[i].issearch = 0;
                    container2.getElementsByClassName(d[i].field + 'sh')[0].classList.add('layui-hide')
                }
            }
            for(let i in conditions){
                if(conditions[i].field==d[index].field){
                    conditions.splice(i,1);
                }
            }
            for(let i in templateDetails){
                if(templateDetails[i].title==d[index].title){
                    templateDetails[i].showSearch=0
                }
            }
        }
        event.stopPropagation();
    });

    form.on("radio()", function (data) {

        for (let j in conditions) {
            if (conditions[j].field + 'sh' == data.elem.name) {
                conditions[j].values = data.value;
            }
        }

    });
    form.on("select()", function (data) {

        for (let j in d) {
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
        for(let i in conditions){
            if(conditions[i].field+'cond'==data.elem.id){
                conditions[i].condition=data.value
            }
            if(conditions[i].field+'sh'==data.elem.id){
                if(data.value!=''){
                    conditions[i].values=data.elem[data.elem.selectedIndex].text;
                }
            }
        }
        for(let i in templateDetails){
            if(templateDetails[i].fieldName+'cond'==data.elem.id){
                templateDetails[i].condition=data.value
            }
        }
    });
    $('#form2Box').click(function () {
        if(_falg){
            $('#form2').slideUp(100);
            _falg=false;
        }else {
            $('#form2').slideDown(100);
            _falg=true
        }
            event.stopPropagation();

    });
    searchBtn.addEventListener('click', function () {
        for (let i in d) {
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
        parm.obj.conditions=conditions;
        if(types==1){
            xc.getInitData(parm);
        }else {
            xc.getActiveTable(parm);
        }

        _falg=false;
        $('#'+options.elem1).parent().slideUp(100);

        let obj = {
            'pagename': pagename,
            'processname':processname,
            'templateDetails': templateDetails,
            'userToken': token, 'signkey': '1f626576304bf5d95b72ece2222e42c3'
        }
        let _parseJson = JSON.stringify(obj);
        $.ajax({
            type: 'post',
            url: '/Query?action=save',
            contentType: "application/json; charset=utf-8",
            data: {parasJson: _parseJson},
            async: false,
            success: function (data) {
                data = JSON.parse(data);
                if (data.result_code == 1) {
                    let ds=data.result_data;
                    templateDetails=[];
                    for(let i in ds){
                        templateDetails.push({'id':ds[i].id,
                            'tempId':ds[i].tempId,
                            'fieldName':ds[i].field,
                            'title':ds[i].title,
                            'dataType':ds[i].type,
                            'condition':ds[i].condition,
                            'width':ds[i].width,
                            'showColume':ds[i].iscolume,
                            'showSearch':ds[i].issearch,
                            'dictId':ds[i].dictId})
                    }
                }else {
                    layer.msg(data.result_msg||data.return_msg)
                }
            }
        })
    })
};

function setOption(j) {
    let options = [{'value': '=', 'key': '0'},
        {'value': '>', 'key': '1'},
        {'value': '<', 'key': '2'},
        {'value': '>=', 'key': '3'},
        {'value': '<=', 'key': '4'},
        {'value': '介于', 'key': '5'},
        {'value': '包含', 'key': '6'}];
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
