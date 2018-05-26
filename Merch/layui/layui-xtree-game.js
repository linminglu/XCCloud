// 本插件依赖 贤心的 layui form模块
// 由 小巷 制作 QQ：151446298
// 2017-10-16 9:40


//构造
function layuiXtree(options) {
    this._container = document.getElementById(options.elem); //容器
    this._dataJson = options.data;  //数据
    this._check=options.checkedArr!=null?options.checkedArr:[];//选中的子节点  [{ID:GameID}]
    this._form = options.form; //layui from对象
    this._domStr = "";  //结构字符串
    this._isopen = options.isopen != null ? options.isopen : true;
    this._color = options.color != null ? options.color : "#12c7c8"; //图标颜色
    if (options.icon == null) options.icon = {};
    this._iconOpen = options.icon.open != null ? options.icon.open : "&#xe625;"; //打开图标
    this._iconClose = options.icon.close != null ? options.icon.close : "&#xe623;"; //关闭图标
    this._iconEnd = options.icon.end != null ? options.icon.end : "&#xe65f;"; //末级图标

    this._single = options.single!= null ? options.single:false; //树节点是否单选
    this.dataBind(this._dataJson);
    this.Rendering( this._single);

}

//生产结构         这是源码，没经过修改
// layuiXtree.prototype.dataBind = function (d) {
//     var _this = this;
//     if (d.length > 0) {
//         for (i in d) {
//             var xtree_isend = '';
//             _this._domStr += '<div class="layui-xtree-item">';
//             if (d[i].data.length > 0)
//                 _this._domStr += '<i class="layui-icon layui-xtree-icon" data-xtree="' + (_this._isopen ? '1' : '0') + '">' + (_this._isopen ? _this._iconOpen : _this._iconClose) + '</i>';
//             else {
//                 _this._domStr += '<i class="layui-icon layui-xtree-icon-null">' + _this._iconEnd + '</i>';
//                 xtree_isend = 'data-xend="1"';
//             }
//             _this._domStr += '<input type="checkbox" class="layui-xtree-checkbox" ' + xtree_isend + ' value="' + d[i].value + '" title="' + d[i].title + '" lay-skin="primary" lay-filter="xtreeck">';
//             _this.dataBind(d[i].data);
//             _this._domStr += '</div>';
//         }
//     }
// }
//生产结构  这里根据自己获得的数据进行修改   title==>name    value==>id   data==>children
layuiXtree.prototype.dataBind = function (d) {
    var _this = this;
    if (d.length > 0) {
        for (i in d) {
            var xtree_isend = '';
            _this._domStr += '<div class="layui-xtree-item">';
            if (d[i].children.length > 0)
                _this._domStr += '<i class="layui-icon layui-xtree-icon" data-xtree="' + (_this._isopen ? '1' : '0') + '">' + (_this._isopen ? _this._iconOpen : _this._iconClose) + '</i>';
            else {
                _this._domStr += '<i class="layui-icon layui-xtree-icon-null">' + _this._iconEnd + '</i>';
                xtree_isend = 'data-xend="1"';
            }
            let flag=false;
            if( _this._check.length>0){
                for( j in _this._check){
                    if(_this._check[j]==d[i].id){
                        flag=true
                    }
                }
            }
            if(flag){
                _this._domStr += '<input type="checkbox" class="layui-xtree-checkbox" checked data-xend="1" '+d[i].disabled?"disabled":""+' value="' + d[i].id + '" title="' + d[i].name + '" lay-skin="primary" lay-filter="xtreeck">';
            }else {
                _this._domStr += '<input type="checkbox" class="layui-xtree-checkbox" ' + xtree_isend + '  '+d[i].disabled ? "disabled" :""+' value="' + d[i].id + '" title="' + d[i].name + '" lay-skin="primary" lay-filter="xtreeck">';
            }
            _this.dataBind(d[i].children);
            _this._domStr += '</div>';
        }
    }
}

//渲染呈现
layuiXtree.prototype.Rendering = function (single) {
    var _this = this;
    _this._container.innerHTML = _this._domStr;
    _this._form.render('checkbox');

    var xtree_items = document.getElementsByClassName('layui-xtree-item');
    var xtree_icons = document.getElementsByClassName('layui-xtree-icon');
    var xtree_nullicons = document.getElementsByClassName('layui-xtree-icon-null');//末端子节点

    for (var i = 0; i < xtree_items.length; i++) {
        if (xtree_items[i].parentNode == _this._container)
            xtree_items[i].style.margin = '5px 0 0 10px';
        else {
            xtree_items[i].style.margin = '5px 0 0 45px';
            if (!_this._isopen) xtree_items[i].style.display = 'none';
        }
    }
    for (var i = 0; i < xtree_icons.length; i++) {
        xtree_icons[i].style.position = "relative";
        xtree_icons[i].style.top = "3px";
        xtree_icons[i].style.margin = "0 5px 0 0";
        xtree_icons[i].style.fontSize = "18px";
        xtree_icons[i].style.color = _this._color;
        xtree_icons[i].style.cursor = "pointer";

        //打开和关闭子树
        xtree_icons[i].onclick = function () {
            var xtree_chi = this.parentNode.childNodes;
            if (this.getAttribute('data-xtree') == 1) {
                for (var j = 0; j < xtree_chi.length; j++) {
                    if (xtree_chi[j].getAttribute('class') == 'layui-xtree-item')
                        xtree_chi[j].style.display = 'none';
                }
                this.setAttribute('data-xtree', '0');
                this.innerHTML = _this._iconClose;
            } else {
                for (var j = 0; j < xtree_chi.length; j++) {
                    if (xtree_chi[j].getAttribute('class') == 'layui-xtree-item')
                        xtree_chi[j].style.display = 'block';
                }
                this.setAttribute('data-xtree', '1');
                this.innerHTML = _this._iconOpen;
            }
        }
    }
    for (var i = 0; i < xtree_nullicons.length; i++) {
        xtree_nullicons[i].style.position = "relative";
        xtree_nullicons[i].style.top = "3px";
        xtree_nullicons[i].style.margin = "0 5px 0 0";
        xtree_nullicons[i].style.fontSize = "18px";
        xtree_nullicons[i].style.color = _this._color;
    }

    _this._form.on('checkbox(xtreeck)', function (da) {
        if(single){
            var xtree_chis = da.elem.parentNode.getElementsByClassName('layui-xtree-item');
            if(xtree_chis.length){
                da.othis[0].classList.remove('layui-form-checked');
                da.elem.checked=false;
            }else {
                var xtree_sib=[];
                for(var i = 0; i < xtree_items.length;i++){
                    if (xtree_items[i] !=da.elem.parentNode){
                        xtree_sib.push(xtree_items[i])
                    }
                }
                for(i in xtree_sib){
                    xtree_sib[i].lastChild.classList.remove('layui-form-checked');
                    xtree_sib[i].children[1].checked=false;
                }
            }

            console.log(xtree_chis);
            //遍历它们，选中状态与它们的父级一致（类似全选功能）
            for (var i = 0; i < xtree_chis.length; i++) {
                xtree_chis[i].getElementsByClassName('layui-xtree-checkbox')[0].checked = da.elem.checked;
                if (da.elem.checked) xtree_chis[i].getElementsByClassName('layui-xtree-checkbox')[0].nextSibling.classList.add('layui-form-checked');
                else xtree_chis[i].getElementsByClassName('layui-xtree-checkbox')[0].nextSibling.classList.remove('layui-form-checked');
            }
        }else {
            console.log(da)
            var  xtree_chis = da.elem.parentNode.getElementsByClassName('layui-xtree-item');

            console.log(xtree_chis);
            //遍历它们，选中状态与它们的父级一致（类似全选功能）
            for (var i = 0; i < xtree_chis.length; i++) {
                xtree_chis[i].getElementsByClassName('layui-xtree-checkbox')[0].checked = da.elem.checked;
                if (da.elem.checked) xtree_chis[i].getElementsByClassName('layui-xtree-checkbox')[0].nextSibling.classList.add('layui-form-checked');
                else xtree_chis[i].getElementsByClassName('layui-xtree-checkbox')[0].nextSibling.classList.remove('layui-form-checked');
            }
        }
        //获取当前点击复选框的容器下面的所有子级容器

        _this.ParendCheck(da.elem);
    });
}

//子阶段选中改变，父节点更改自身状态
layuiXtree.prototype.ParendCheck = function (ckelem) {
    var xtree_p = ckelem.parentNode.parentNode;
    if (xtree_p.getAttribute('class') == 'layui-xtree-item') {
        var xtree_all = xtree_p.getElementsByClassName('layui-xtree-item');
        var xtree_count = 0;

        for (var i = 0; i < xtree_all.length; i++) {
            if (xtree_all[i].getElementsByClassName('layui-xtree-checkbox')[0].checked) {
                xtree_count++;
            }
        }

        if (xtree_count <= 0) {
            xtree_p.getElementsByClassName('layui-xtree-checkbox')[0].checked = false;
            xtree_p.getElementsByClassName('layui-xtree-checkbox')[0].nextSibling.classList.remove('layui-form-checked');
        } else {
            xtree_p.getElementsByClassName('layui-xtree-checkbox')[0].checked = true;
            xtree_p.getElementsByClassName('layui-xtree-checkbox')[0].nextSibling.classList.add('layui-form-checked');
        }
        this.ParendCheck(xtree_p.getElementsByClassName('layui-xtree-checkbox')[0]);
    }
}

//获取全部选中的末级checkbox对象
layuiXtree.prototype.GetChecked = function () {
    var arr = new Array();
    var arrIndex = 0;
    var cks = document.getElementsByClassName('layui-xtree-checkbox');
    for (var i = 0; i < cks.length; i++) {
        if (cks[i].checked && cks[i].getAttribute('data-xend') == '1') {
            arr[arrIndex] = cks[i]; arrIndex++;
        }
    }
    return arr;
}