const { chromium } = require('playwright');

(async () => {
    const browser = await chromium.launch({ headless: false });
    const context = await browser.newContext();
    const page = await context.newPage();

    try {
        console.log('Đang điều hướng đến trang chủ...');
        await page.goto('http://localhost:5198');
        await page.waitForLoadState('networkidle');
        
        console.log('Trang đã tải. Đang chụp màn hình ban đầu...');
        await page.screenshot({ path: 'initial-page.png' });
        
        console.log('Đang kiểm tra chat widget...');
        
        // Kiểm tra xem có chat fab không
        const chatFab = await page.$('.chat-fab');
        if (chatFab) {
            console.log('Chat fab đã xuất hiện ✓');
            
            // Mở chat widget
            await page.click('.chat-fab');
            console.log('Đã mở chat widget');
            
            // Đợi chat box xuất hiện
            await page.waitForSelector('.chat-box', { timeout: 5000 });
            console.log('Chat box đã xuất hiện');
            
            // Kiểm tra nút đính kèm file
            const attachButton = await page.$('.chat-attach');
            if (attachButton) {
                console.log('Nút đính kèm file đã có ✓');
            } else {
                console.log('Nút đính kèm file KHÔNG có ✗');
            }
            
            // Kiểm tra file input
            const fileInput = await page.$('input[type="file"]');
            if (fileInput) {
                console.log('File input đã có ✓');
            } else {
                console.log('File input KHÔNG có ✗');
            }
            
            // Chụp màn hình sau khi mở chat
            await page.screenshot({ path: 'chat-widget-opened.png' });
            console.log('Đã chụp màn hình: chat-widget-opened.png');
        } else {
            console.log('Chat fab KHÔNG xuất hiện ✗');
            console.log('Có thể chat widget chỉ hiển thị cho user không phải admin');
            console.log('Hoặc cần đăng nhập để hiển thị chat widget');
            
            // Chụp màn hình để kiểm tra
            await page.screenshot({ path: 'no-chat-fab.png' });
            console.log('Đã chụp màn hình: no-chat-fab.png');
        }
        
        console.log('\n=== Kết quả test ===');
        console.log('✓ Test hoàn thành');
        
    } catch (error) {
        console.error('Lỗi trong quá trình test:', error.message);
        
        // Chụp màn hình lỗi nếu có thể
        try {
            await page.screenshot({ path: 'error-screenshot.png' });
            console.log('Đã chụp màn hình lỗi: error-screenshot.png');
        } catch (e) {
            // Ignore screenshot errors
        }
    } finally {
        await browser.close();
    }
})();
