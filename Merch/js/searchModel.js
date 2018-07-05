var aa=(function () {
    // 构造
   var seachModel =function (options) {
        let _this=this;
        _this.container1 = document.getElementById(options.elem1); //容器
        _this.container2 = document.getElementById(options.elem2); //容器
        _this.pagename = options.pagename;  //数据
        _this.processname = options.processname;  //数据

        _this.token=options.token;//选中的子节点  [{ID:GameID}]
        _this.form = options.form; //layui from对象
        _this.layer = options.layer; //layui from对象
        _this._data=[];
        _this._domStr1 = "";  //结构字符串
        _this._domStr2 = "";  //结构字符串
        // this.getData(_this._data);
        aa();
        _this.initCheckBox(_this._data);
        _this.initSerach(_this._data);
        _this.rendering(_this._data);
        console.log(this)
    }
//获取参数
    seachModel.prototype.getData=function (d) {
        var _this=this;
        let obj={
            'pagename':_this.pagename,
            'processname':_this.processname,
            'userToken':_this.token,'signkey':'1f626576304bf5d95b72ece2222e42c3'
        }
        let _parseJson = JSON.stringify(obj);
        $.ajax({
            type:'post',
            url:'/Query?action=init',
            contentType: "application/json; charset=utf-8",
            data:{parasJson: _parseJson},
            async:false,
            success: function (data) {
                data = JSON.parse(data);
                if (data.result_code == 1) {
                    d=data.result_data;

                } else {
                    _this.layer.msg(data.result_msg||data.return_msg);
                }
            }
        });
    }
//生成下拉复选框
    seachModel.prototype.initCheckBox=function (d) {
        var _this=this;
        if(d.length>0){
            for(i in d){
                if(d[i].issearch==1){
                    _this._domStr1+='<input type="checkbox" checked lay-skin="primary" value="'+d[i].id+'" title="'+d[i].title+'" lay-filter="checkList"/><br>'
                }else {
                    _this._domStr1+='<input type="checkbox" lay-skin="primary" value="'+d[i].id+'" title="'+d[i].title+'" lay-filter="checkList"/><br>'
                }

            }
        }

    }
//生成查询条件框
    seachModel.prototype.initSerach=function (d) {
        var _this=this;
        if(d.length>0){
            for(i in d){
                if(d[i].type=='literals'){
                    _this._domStr2+= '<div class="layui-inline">' +
                        '<label class="layui-form-label">'+d[i].title+'</label>' ;
                    // '<div class="layui-input-inline">';
                    // _this._domStr2+=<div class="layui-input-inline"><select>setOption(d[i].condition)</select></div>;
                    // _this._domStr2+= '</select></div>';
                    _this._domStr2+='<div class="layui-input-inline"><select id="'+d[i].field+'" lay-filter="'+d[i].field+'">';
                    let html='<option value="">-请选择-</option>';
                    for(j in d[i].list){
                        html+='<option value="'+j+'">'+d[i].list[j]+'</option>'
                    }
                    _this._domStr2+=html;
                    _this._domStr2+= '</select></div></div>';
                }else if(d[i].type=='bit'){
                    _this._domStr2+=' <div class="layui-inline '+d[i].field+'"><label class="layui-form-label">'+d[i].title+'</label>' +
                        '<div class="layui-input-inline">' +
                        '<input type="radio" name="sex" value="1" title="是" checked="" lay-filter="'+d[i].field+'">' +
                        '<input type="radio" name="sex" value="0" title="否" lay-filter="'+d[i].field+'">' +
                        '</div></div>'
                }else if(d[i].type=='string'){
                    _this._domStr2+=' <div class="layui-inline '+d[i].field+'"><label class="layui-form-label">'+d[i].title+'</label>' +
                        '<div class="layui-input-inline">' +
                        '<input type="text" id="'+d[i].field+'">' +
                        '</div>'
                }else if(d[i].type=='number'){
                    if(d[i].condition==5){
                        _this._domStr2+=' <div class="layui-inline '+d[i].field+'"><label class="layui-form-label">'+d[i].title+'</label>' +
                            '<div class="layui-input-inline"><select>'+setOption(d[i].condition)+'</select></div>'+
                            '<div class="layui-input-inline">' +
                            '<input type="text" id="'+d[i].field+'1">' +
                            '</div>' +
                            '<div class="layui-form-mid">至</div>' +
                            '<div class="layui-input-inline">' +
                            '<input type="text" id="'+d[i].field+'2">' +
                            '</div>' +
                            '</div>'
                    }else {
                        _this._domStr2+=' <div class="layui-inline '+d[i].field+'"><label class="layui-form-label">'+d[i].title+'</label>' +
                            '<div class="layui-input-inline"><select>'+setOption(d[i].condition)+'</select></div>'+
                            '<div class="layui-input-inline">' +
                            '<input type="text" id="'+d[i].field+'1">' +
                            '<input type="text" id="'+d[i].field+'2">' +
                            '</div></div>'
                    }

                }else if(d[i].type=='time'){
                    if(d[i].condition==5){
                        _this._domStr2+=' <div class="layui-inline '+d[i].field+'"><label class="layui-form-label">'+d[i].title+'</label>' +
                            '<div class="layui-input-inline"><select>'+setOption(d[i].condition)+'</select></div>'+
                            '<div class="layui-input-inline">' +
                            '<input type="text" id="'+d[i].field+'1">' +
                            '</div>' +
                            '<div class="layui-form-mid">至</div>' +
                            '<div class="layui-input-inline">' +
                            '<input type="text" id="'+d[i].field+'2">' +
                            '</div>' +
                            '</div>'
                    }else {
                        _this._domStr2+=' <div class="layui-inline '+d[i].field+'"><label class="layui-form-label">'+d[i].title+'</label>' +
                            '<div class="layui-input-inline"><select>'+setOption(d[i].condition)+'</select></div>'+
                            '<div class="layui-input-inline">' +
                            '<input type="text" id="'+d[i].field+'1">' +
                            '</div></div>'
                    }
                }else if(d[i].type=='data'){
                    if(d[i].condition==5){
                        _this._domStr2+=' <div class="layui-inline"><label class="layui-form-label">'+d[i].title+'</label>' +
                            '<div class="layui-input-inline"><select>'+setOption(d[i].condition)+'</select></div>'+
                            '<div class="layui-input-inline">' +
                            '<input type="text" id="'+d[i].field+'1">' +
                            '</div>' +
                            '<div class="layui-form-mid">至</div>' +
                            '<div class="layui-input-inline">' +
                            '<input type="text" id="'+d[i].field+'2">' +
                            '</div>' +
                            '</div>'
                    }else {
                        _this._domStr2+=' <div class="layui-inline"><label class="layui-form-label">'+d[i].title+'</label>' +
                            '<div class="layui-input-inline"><select>'+setOption(d[i].condition)+'</select></div>'+
                            '<div class="layui-input-inline">' +
                            '<input type="text" id="'+d[i].field+'1">' +
                            '</div></div>'
                    }
                }

            }
        }

    }
    seachModel.prototype.rendering=function (d) {
        var _this=this;
        _this.container1.innerHTML=_this._domStr1;
        _this.container2.innerHTML=_this._domStr2;
        _this.form.render();
        form.on('checkbox(checkList)',function (data) {
            let _val=data.value;
            if(data.elem.checked){
                for( i in d){
                    if(d[i].id==_val){
                        d[i].issearch=1;
                        _this.container2.getElementsByClassName(d[i].field)[0].classList.remove('layui-hide')
                    }
                }
            }else {
                for( i in d){
                    if(d[i].id==_val){
                        d[i].issearch=0
                    }
                    _this.container2.getElementsByClassName(d[i].field)[0].classList.add('layui-hide')
                }
            }
        })
    }
    function setOption(j) {
        let options=[{'value':'=','key':'0'},
            {'value':'>','key':'1'},
            {'value':'<','key':'2'},
            {'value':'>=','key':'3'},
            {'value':'<=','key':'4'},
            {'value':'介于','key':'5'},
            {'value':'包含','key':'6'}]
        let html='';
        for(let i in options){
            if(options[i].key==j){
                html+='<option value="'+options[i].key+' selected">'+options[i].value+'</option>'
            }else {
                html+='<option value="'+options[i].key+'">'+options[i].value+'</option>'
            }
        }
        return html;
    }
    // window.seachModel=seachModel;window
})()

aa()
// let token1=window.localStorage.getItem('token');
// layui.use(['form','layer','table'],()=> {
//     const form = layui.form;
//     const layer = layui.layer;
//     const table = layui.table;
//
//     var searchM= new seachModel({
//         'elem1': 'tantion',
//         'elem2': 'serchMode',
//         'pagename': 'projectInfoSearch',
//         'processname': 'projectInfoSearch',
//         'token': token1,
//         'form': form,
//         'layer': layer
//     })
// })