// Auto-uppercase department code
const deptCodeInput = document.getElementById('deptCode');
if (deptCodeInput) {
    deptCodeInput.addEventListener('input', function(e) {
        this.value = this.value.toUpperCase();
    });
}

// Form validation
const form = document.getElementById('createDeptForm');
if (form) {
    form.addEventListener('submit', function(e) {
        const deptName = document.querySelector('input[name="DepartmentName"]').value.trim();
        const deptCode = document.querySelector('input[name="DepartmentCode"]').value.trim();
        
        if (!deptName || !deptCode) {
            e.preventDefault();
            alert('Please fill in all required fields (Department Name and Code)');
            return false;
        }
        
        // Show loading state
        const submitBtn = form.querySelector('button[type="submit"]');
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Creating...';
        
        console.log('Creating department:', {
            DepartmentName: deptName,
            DepartmentCode: deptCode
        });
    });
}

console.log('Create Department Form Loaded');
