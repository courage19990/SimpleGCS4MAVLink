let qE = function(str) {
	return document.querySelector(str);
};

let addRel = function(dom, action, method) {
	if(typeof dom === "string") dom = qE(dom);
	dom.addEventListener(action, method);
};

let getParamFromUrl = function(paramName) {
	let urL = location.href;
	let params = urL.slice(urL.indexOf("?") + 1);
	params = params.split("&");
	for(let [name, value] of params.map(x => x.split("="))) {
		if(name === paramName) return value;
	}
};

function elt(name, attrs, ...children) { 
	// ...操作符，把[a,b,c]打散为a,b,c，打散调用children时仍然要...
	let dom = document.createElement(name);
	for(let attr of Object.keys(attrs)) {
		dom.setAttribute(attr, attrs[attr]);
	}
	for(let child of children) {
		if(["string", "number"].indexOf(typeof child) == -1) {
			dom.appendChild(child);
		} else {
			dom.appendChild(document.createTextNode(child));
		}
	}
	return dom;
}