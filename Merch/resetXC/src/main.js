import Vue from 'vue'
import ElemerntUI from 'element-ui'
import 'element-ui/lib/theme-chalk/index.css'
import App from './App'
import router from './router'
// import Login from './components/Login.vue'
// import Home from '@/components/Home.vue'
import axios from 'axios'

Vue.use(ElemerntUI)
Vue.prototype.$axios=axios
Vue.config.productionTip = false
// Vue.prototype.HOST='/api'

/* eslint-disable no-new */
new Vue({
  el: '#app',
  router,
  components:{App},
  template:'<App/>',
  render: h => h(App)
})
