// $(function () {
    var chart = Highcharts.chart('container', {
        chart: {
            type: 'column'
        },
        title: {
            text: ''
        },
        // subtitle: {
        //     text: '请点击按钮查看坐标轴变化'
        // },
        xAxis: {
            categories: ['一月', '二月', '三月', '四月', '五月', '六月',
                '七月', '八月', '九月', '十月', '十一月', '十二月']
        },
        yAxis: {
            labels: {
                x: -15
            },
            title: {
                text: '营业额'
            }
        },
        series: [{
            name: '机台名称',
            data: [434, 523, 345, 785, 565, 843, 726, 590, 665, 434, 312, 432]
        }],
        responsive: {
            rules: [{
                condition: {
                    maxWidth: 500
                },
                // Make the labels less space demanding on mobile
                chartOptions: {
                    xAxis: {
                        labels: {
                            formatter: function () {
                                return this.value.replace('月', '')
                            }
                        }
                    },
                    yAxis: {
                        labels: {
                            align: 'left',
                            x: 0,
                            y: -2
                        },
                        title: {
                            text: ''
                        }
                    }
                }
            }]
        }
    });

//画百分比圆环
    var drawCricle=function (id,ratio,text,color) {
        var ctx = document.getElementById(id).getContext('2d');
        // var ctx = document.getElementById('radius').getContext('2d');
        //画笔设置
        ctx.strokeStyle = "#eee";
        ctx.lineWidth = "9";
        ctx.lineCap = 'round';
        // //画星星
        // var img = new Image();
        // img.src = "star.png";
        // img.onload = function () {
        //     ctx.drawImage(img, 40, 40, 28, 28);
        // };
        //灰色圆环
        ctx.beginPath();
        ctx.arc(75, 75, 50, 0,2*Math.PI);
        ctx.stroke();
        ctx.closePath();
        //白色圆环
        drawArc(ratio, ctx, text,color);
    }

        //根据角度比例画圆弧，和奖杯个数
        function drawArc(ratio, ctx, text,color){
            //设置
            ctx.strokeStyle = color;
            var endAngle =  ((2 * ratio)-0.5) * Math.PI;
            ctx.beginPath();
            ctx.arc(75, 75, 50, -0.5*Math.PI, endAngle, false);
            ctx.stroke();
            ctx.closePath();

            var myText = ctx.measureText(text);

            ctx.fillStyle = color;
            ctx.font = "18px Microsoft YaHei";
            ctx.fillText(text, (150 - myText.width)/2 - 2, 85);
        }


// drawCricle('allPresent');
// })
